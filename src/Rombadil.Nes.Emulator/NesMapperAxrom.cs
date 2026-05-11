namespace Rombadil.Nes.Emulator;

public class NesMapperAxrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private byte selectedBank;

    public NesMapperAxrom(Memory<byte> prg, Memory<byte> chr)
    {
        this.prg = prg;
        this.chr = chr;
        mirroring = NesMirroring.SingleScreenLow;
    }

    public override void Write(ushort addr, byte value)
    {
        selectedBank = (byte)(value & 0x07);
        mirroring = (value & 0x10) != 0 ? NesMirroring.SingleScreenHigh : NesMirroring.SingleScreenLow;
    }

    public override byte Read(ushort addr)
    {
        int bank = selectedBank * 0x8000;
        return prg.Span[(bank + (addr - 0x8000)) % prg.Length];
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        return chr.Span[addr % chr.Length];
    }
}
