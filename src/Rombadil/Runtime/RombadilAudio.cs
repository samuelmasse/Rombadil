namespace Rombadil;

[Rombadil]
public unsafe class RombadilAudio : IDisposable
{
    private const int AudioFreq = 44100;
    private const int InternalSampleRate = 96000;
    private const int AudioChunkSize = 173;
    private const int MinimumAudioBufferCount = 12;
    private const int MinimumPlayGate = 9;
    private const int MaxAudioBufferCount = 256;
    private const double MinimumMaxLatencyMs = 70;
    private const double ChunkMs = AudioChunkSize * 1000.0 / AudioFreq;

    private readonly Ma ma;
    private readonly Lock audioLock = new();
    private readonly MaDeviceDataProc dataCallback;
    private readonly Blip blip;
    private readonly HermiteResampler hermite;
    private readonly List<int> samples = [];
    private readonly Queue<int> freeBuffers = new();
    private readonly short[] audioChunk = new short[AudioChunkSize];

    private MaDevice* device;
    private AudioBuffer[] buffers = [];
    private int[] queuedBuffers = [];
    private int queuedBufferCount;
    private int processedBufferCount;
    private int activeBufferOffset;
    private int playGate = MinimumPlayGate;
    private int hardLowWatermark = 3;
    private int softLowWatermark = 6;
    private int softHighWatermark = 8;
    private int hardHighWatermark = 11;
    private double maxLatencyMs = MinimumMaxLatencyMs;
    private int lastMix;
    private bool playing;
    private bool playbackWasActive;
    private double speedMultiplier;
    private DateTime speedMultiplierTime;

    public List<int> Samples => samples;

    public RombadilAudio(Ma ma)
    {
        this.ma = ma;
        dataCallback = DataCallback;
        blip = new Blip(0xFFFFF, RombadilNesTiming.CpuHz, InternalSampleRate);
        hermite = new HermiteResampler(blip, InternalSampleRate, AudioFreq);
    }

    public void Start()
    {
        if (device != null)
            return;

        device = (MaDevice*)NativeMemory.AllocZeroed((nuint)sizeof(MaDevice));
        var initialized = false;

        try
        {
            var config = ma.DeviceConfigInit(MaDeviceType.DeviceTypePlayback);
            config.Playback.Format = MaFormat.FormatS16;
            config.Playback.Channels = 1;
            config.SampleRate = AudioFreq;
            config.DataCallback = Marshal.GetFunctionPointerForDelegate(dataCallback);
            config.PerformanceProfile = MaPerformanceProfile.PerformanceProfileLowLatency;

            Require("ma_device_init", ma.DeviceInit(null, &config, device));
            initialized = true;
            ConfigureBuffers(device->Playback.InternalPeriodSizeInFrames);
            Require("ma_device_start", ma.DeviceStart(device));
        }
        catch
        {
            if (initialized)
                ma.DeviceUninit(device);

            NativeMemory.Free(device);
            device = null;
            ResetQueuedBuffers();
            throw;
        }
    }

    public void Pump(double effectiveSpeed)
    {
        if (device == null)
        {
            samples.Clear();
            return;
        }

        ComputeDeltas();

        int queued = UnqueueProcessedBuffers();
        effectiveSpeed *= UpdateSpeedMultiplier(queued, DateTime.UtcNow);
        var latency = ObservedLatencyMs(queued, effectiveSpeed);

        if (latency > maxLatencyMs)
            Drop();

        RefillBuffers(effectiveSpeed);
        ApplyPlayGate();
    }

    public void Drop()
    {
        lock (audioLock)
        {
            playing = false;
            playbackWasActive = false;

            for (var i = 0; i < queuedBufferCount; i++)
                freeBuffers.Enqueue(queuedBuffers[i]);

            queuedBufferCount = 0;
            processedBufferCount = 0;
            activeBufferOffset = 0;
        }

        blip.Clear();
        samples.Clear();
    }

    private void DataCallback(MaDevice* pDevice, nint pOutput, nint pInput, uint frameCount)
    {
        _ = pDevice;
        _ = pInput;

        if (pOutput == 0 || frameCount == 0)
            return;

        var output = new Span<short>((void*)pOutput, checked((int)frameCount));
        var outputOffset = 0;
        var remaining = output.Length;

        lock (audioLock)
        {
            if (playing)
                DrainQueuedBuffers(output, ref outputOffset, ref remaining);

            if (playing && processedBufferCount >= queuedBufferCount)
                playing = false;
        }

        if (remaining > 0)
            output.Slice(outputOffset, remaining).Clear();
    }

    private void DrainQueuedBuffers(Span<short> output, ref int outputOffset, ref int remaining)
    {
        while (remaining > 0 && processedBufferCount < queuedBufferCount)
        {
            var buffer = buffers[queuedBuffers[processedBufferCount]];
            var available = buffer.Length - activeBufferOffset;
            var count = Math.Min(available, remaining);

            buffer.Data.AsSpan(activeBufferOffset, count).CopyTo(output.Slice(outputOffset, count));
            outputOffset += count;
            remaining -= count;
            activeBufferOffset += count;

            if (activeBufferOffset < buffer.Length)
                continue;

            processedBufferCount++;
            activeBufferOffset = 0;
        }
    }

    private void ConfigureBuffers(uint devicePeriodSizeInFrames)
    {
        var gulp = devicePeriodSizeInFrames > 0
            ? devicePeriodSizeInFrames > int.MaxValue ? int.MaxValue : (int)devicePeriodSizeInFrames
            : AudioChunkSize;

        var chunksPerGulp = (gulp + AudioChunkSize - 1) / AudioChunkSize;
        var gate = (3 * chunksPerGulp + 1) / 2;
        var count = gate + chunksPerGulp + 1;

        if (gate < MinimumPlayGate)
            gate = MinimumPlayGate;

        if (count < gate + 3)
            count = gate + 3;

        if (count < MinimumAudioBufferCount)
            count = MinimumAudioBufferCount;

        if (count > MaxAudioBufferCount)
            count = MaxAudioBufferCount;

        if (gate > count - 1)
            gate = count - 1;

        maxLatencyMs = count * ChunkMs + gulp * 1000.0 / AudioFreq + 30;
        if (maxLatencyMs < MinimumMaxLatencyMs)
            maxLatencyMs = MinimumMaxLatencyMs;

        hardLowWatermark = count / 4;
        softLowWatermark = count / 2;
        softHighWatermark = count * 2 / 3;
        hardHighWatermark = count * 11 / 12;
        playGate = gate;

        ResetQueuedBuffers(count);
    }

    private void ResetQueuedBuffers(int bufferCount = 0)
    {
        lock (audioLock)
        {
            buffers = new AudioBuffer[bufferCount];
            queuedBuffers = new int[bufferCount];
            freeBuffers.Clear();

            for (var i = 0; i < bufferCount; i++)
            {
                buffers[i] = new AudioBuffer();
                freeBuffers.Enqueue(i);
            }

            queuedBufferCount = 0;
            processedBufferCount = 0;
            activeBufferOffset = 0;
            playing = false;
            playbackWasActive = false;
        }
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

    private int UnqueueProcessedBuffers()
    {
        lock (audioLock)
        {
            if (playing || playbackWasActive)
            {
                var processed = Math.Min(processedBufferCount, queuedBufferCount);
                for (var i = 0; i < processed; i++)
                    freeBuffers.Enqueue(queuedBuffers[i]);

                var remaining = queuedBufferCount - processed;
                if (remaining > 0 && processed > 0)
                    Array.Copy(queuedBuffers, processed, queuedBuffers, 0, remaining);

                queuedBufferCount = remaining;
                processedBufferCount = 0;
            }

            return queuedBufferCount;
        }
    }

    private double UpdateSpeedMultiplier(int queued, DateTime time)
    {
        double targetMultiplier = 1;

        if (queued < hardLowWatermark)
            targetMultiplier = 1 - 1 / 256f;
        else if (queued < softLowWatermark)
            targetMultiplier = 1 - 1 / 512f;
        else if (queued > hardHighWatermark)
            targetMultiplier = 1 + 1 / 256f;
        else if (queued > softHighWatermark)
            targetMultiplier = 1 + 1 / 512f;

        if (targetMultiplier != speedMultiplier && (time - speedMultiplierTime).TotalSeconds > 1)
        {
            speedMultiplier = targetMultiplier;
            speedMultiplierTime = time;
        }

        return speedMultiplier;
    }

    private double ObservedLatencyMs(int queued, double effectiveSpeed)
    {
        if (effectiveSpeed <= 0)
            effectiveSpeed = 1;

        double blipDrainRate = InternalSampleRate * effectiveSpeed;
        return blip.SamplesAvail * 1000.0 / blipDrainRate
            + queued * AudioChunkSize * 1000.0 / AudioFreq;
    }

    private void RefillBuffers(double effectiveSpeed)
    {
        int neededBlipSamples = (int)Math.Ceiling(
            AudioChunkSize * (double)InternalSampleRate / AudioFreq * effectiveSpeed) + 1;

        while (freeBuffers.Count > 0 && blip.SamplesAvail >= neededBlipSamples)
        {
            int read = hermite.Read(audioChunk.AsSpan(0, AudioChunkSize), effectiveSpeed);
            if (read == 0)
                break;

            int bufferIndex = freeBuffers.Dequeue();
            buffers[bufferIndex].Set(audioChunk.AsSpan(0, read));

            lock (audioLock)
                queuedBuffers[queuedBufferCount++] = bufferIndex;

            if (read < AudioChunkSize)
                break;
        }
    }

    private void ApplyPlayGate()
    {
        lock (audioLock)
        {
            if (!playing && queuedBufferCount > playGate)
                playing = true;

            playbackWasActive = playing;
        }
    }

    public void Dispose()
    {
        if (device == null)
            return;

        ma.DeviceUninit(device);
        NativeMemory.Free(device);
        device = null;
        ResetQueuedBuffers();
    }

    private void Require(string name, MaResult result)
    {
        if (result == MaResult.Success)
            return;

        ma.ResultDescription(result, out var description);
        throw new InvalidOperationException($"{name} failed: {description ?? result.ToString()}.");
    }

    private sealed class AudioBuffer
    {
        public short[] Data { get; } = new short[AudioChunkSize];
        public int Length { get; private set; }

        public void Set(ReadOnlySpan<short> source)
        {
            source.CopyTo(Data);
            Length = source.Length;
        }
    }
}
