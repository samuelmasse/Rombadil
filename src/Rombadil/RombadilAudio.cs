namespace Rombadil;

public class RombadilAudio : IDisposable
{
    private const int AudioFreq = 44100;
    private const int InternalSampleRate = 96000;
    private const int AudioChunkSize = 173;
    private const int AudioBufferCount = 12;
    private const double MaxLatencyMs = 60;

    private readonly Blip blip;
    private readonly HermiteResampler hermite;
    private readonly List<int> samples = [];
    private readonly Queue<int> freeBuffers = new();
    private readonly short[] audioChunk = new short[AudioChunkSize];

    private ALDevice device;
    private ALContext context;
    private int source;
    private int[] buffers = [];
    private int lastMix;
    private bool sourceWasPlaying;
    private double speedMultiplier;
    private DateTime speedMultiplierTime;

    public List<int> Samples => samples;

    public RombadilAudio(double clockRate)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            OpenALLibraryNameContainer.OverridePath = "libopenal.1.dylib";

        var assembly = Assembly.GetExecutingAssembly();
        var iniPath = Path.Combine(AppContext.BaseDirectory, "alsoft.ini");
        if (!File.Exists(iniPath))
        {
            using var stream = assembly.GetManifestResourceStream("Rombadil.alsoft.ini")!;
            using var reader = new StreamReader(stream);
            File.WriteAllText(iniPath, reader.ReadToEnd());
        }

        blip = new Blip(0xFFFFF, clockRate, InternalSampleRate);
        hermite = new HermiteResampler(blip, InternalSampleRate, AudioFreq);
    }

    public void Start()
    {
        device = ALC.OpenDevice(null);
        context = ALC.CreateContext(device, (int[])null!);
        ALC.MakeContextCurrent(context);

        source = AL.GenSource();
        buffers = AL.GenBuffers(AudioBufferCount);

        foreach (var buffer in buffers)
            freeBuffers.Enqueue(buffer);
    }

    public void Pump(double effectiveSpeed)
    {
        ComputeDeltas();

        AL.GetSource(source, ALGetSourcei.SourceState, out int stateAtStart);
        bool playing = (ALSourceState)stateAtStart == ALSourceState.Playing;

        if (playing || sourceWasPlaying)
        {
            AL.GetSource(source, ALGetSourcei.BuffersProcessed, out int processed);
            for (int i = 0; i < processed; i++)
                freeBuffers.Enqueue(AL.SourceUnqueueBuffer(source));
        }

        var time = DateTime.UtcNow;
        AL.GetSource(source, ALGetSourcei.BuffersQueued, out int queued);
        double targetMultiplier = 1;

        if (queued < 3)
            targetMultiplier = 1 - 1 / 256f;
        else if (queued < 6)
            targetMultiplier = 1 - 1 / 512f;
        else if (queued > 11)
            targetMultiplier = 1 + 1 / 256f;
        else if (queued > 8)
            targetMultiplier = 1 + 1 / 512f;

        if (targetMultiplier != speedMultiplier && (time - speedMultiplierTime).TotalSeconds > 1)
        {
            speedMultiplier = targetMultiplier;
            speedMultiplierTime = time;
        }

        effectiveSpeed *= speedMultiplier;
        var latency = ObservedLatencyMs(effectiveSpeed);

        if (latency > MaxLatencyMs)
        {
            AL.SourceStop(source);
            AL.GetSource(source, ALGetSourcei.BuffersQueued, out int queuedToDrop);

            for (int i = 0; i < queuedToDrop; i++)
                freeBuffers.Enqueue(AL.SourceUnqueueBuffer(source));

            blip.Clear();
        }

        int neededBlipSamples = (int)Math.Ceiling(
            AudioChunkSize * (double)InternalSampleRate / AudioFreq * effectiveSpeed) + 1;

        while (freeBuffers.Count > 0 && blip.SamplesAvail >= neededBlipSamples)
        {
            int read = hermite.Read(audioChunk.AsSpan(0, AudioChunkSize), effectiveSpeed);
            if (read == 0)
                break;

            int buffer = freeBuffers.Dequeue();
            var data = (ReadOnlySpan<short>)audioChunk.AsSpan(0, read);
            AL.BufferData(buffer, ALFormat.Mono16, data, AudioFreq);
            AL.SourceQueueBuffer(source, buffer);

            if (read < AudioChunkSize)
                break;
        }

        AL.GetSource(source, ALGetSourcei.SourceState, out int state);

        if ((ALSourceState)state != ALSourceState.Playing)
        {
            AL.GetSource(source, ALGetSourcei.BuffersQueued, out int ready);
            if (ready > 9)
                AL.SourcePlay(source);
        }

        AL.GetSource(source, ALGetSourcei.SourceState, out state);
        sourceWasPlaying = (ALSourceState)state == ALSourceState.Playing;
    }

    private void ComputeDeltas()
    {
        for (int i = 0; i < samples.Count; i++)
        {
            int currentMix = samples[i];
            int delta = currentMix - lastMix;
            if (delta == 0)
                continue;

            blip.AddDelta((uint)i, delta);
            lastMix = currentMix;
        }

        blip.EndFrame((uint)samples.Count);
        samples.Clear();
    }

    private double ObservedLatencyMs(double effectiveSpeed)
    {
        AL.GetSource(source, ALGetSourcei.BuffersQueued, out int queued);
        AL.GetSource(source, ALGetSourcei.BuffersProcessed, out int processed);
        int alPendingSamples = (queued - processed) * AudioChunkSize;
        double blipDrainRate = InternalSampleRate * effectiveSpeed;
        return blip.SamplesAvail * 1000.0 / blipDrainRate
            + alPendingSamples * 1000.0 / AudioFreq;
    }

    public void Dispose()
    {
        AL.SourceStop(source);
        AL.DeleteSource(source);
        AL.DeleteBuffers(buffers);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
    }
}
