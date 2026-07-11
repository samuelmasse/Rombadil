namespace Rombadil.Nes.Emulator;

public sealed class NesBatteryRam(int length)
{
    private readonly byte[] data = new byte[length];

    public int Length => data.Length;
    public bool Dirty { get; private set; }

    public byte Read(int index) => data[index];

    public void Write(int index, byte value)
    {
        if (data[index] == value)
            return;

        data[index] = value;
        Dirty = true;
    }

    public void Load(ReadOnlySpan<byte> source)
    {
        if (source.Length != data.Length)
            throw new ArgumentException($"Expected {data.Length} save bytes, received {source.Length}.", nameof(source));

        source.CopyTo(data);
        Dirty = false;
    }

    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length != data.Length)
            throw new ArgumentException($"Expected a {data.Length}-byte destination, received {destination.Length}.", nameof(destination));

        data.CopyTo(destination);
    }

    public void MarkClean() => Dirty = false;
}
