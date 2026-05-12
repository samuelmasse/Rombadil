namespace Rombadil.Nes.Emulator;

public readonly struct NesRomHeader(Memory<byte> header)
{
    public byte PrgRomSize => header.Span[4];
    public byte ChrRomSize => header.Span[5];
    public int MapperNumber => (Flags7 & 0xF0) | (Flags6 >> 4) | (IsNes20 ? (Flags8 & 0x0F) << 8 : 0);
    public byte Submapper => IsNes20 ? (byte)(Flags8 >> 4) : (byte)0;

    public bool VerticalMirroring => (Flags6 & 0x01) != 0;
    public bool HasBattery => (Flags6 & 0x02) != 0;
    public bool HasTrainer => (Flags6 & 0x04) != 0;
    public bool FourScreen => (Flags6 & 0x08) != 0;
    public bool IsNes20 => (Flags7 & 0x0C) == 0x08;

    private byte Flags6 => header.Span[6];
    private byte Flags7 => header.Span[7];
    private byte Flags8 => header.Span[8];
}
