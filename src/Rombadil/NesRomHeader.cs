namespace Rombadil;

public readonly struct NesRomHeader(Memory<byte> header)
{
    public byte PrgRomSize => header.Span[4];
    public byte ChrRomSize => header.Span[5];
    public byte MapperNumber => (byte)((Flags7 & 0xF0) | (Flags6 >> 4));

    private byte Flags6 => header.Span[6];
    private byte Flags7 => header.Span[7];
}
