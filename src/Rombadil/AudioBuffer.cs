namespace Rombadil.Nes.Emulator;

public class AudioBuffer
{
    private readonly short[][] buffers;
    private readonly int[] counts;
    private long inputIndex = 1;
    private long outputIndex;

    public ReadOnlySpan<short> Output => buffers[outputIndex % buffers.Length].AsSpan()[..counts[outputIndex % buffers.Length]];

    public long Delay => inputIndex - outputIndex;

    public AudioBuffer(int unit, int size)
    {
        counts = new int[size];
        buffers = new short[size][];
        for (int i = 0; i < size; i++)
            buffers[i] = new short[unit];
    }

    public void Add(short val)
    {
        long bufferIndex = inputIndex % buffers.Length;
        ref var count = ref counts[bufferIndex];
        buffers[bufferIndex][count] = val;
        count++;
    }

    public void Submit()
    {
        inputIndex++;
        counts[inputIndex % buffers.Length] = 0;
    }

    public void Retrieve()
    {
        outputIndex++;
    }
}
