namespace Rombadil.Nes.Emulator;

public class NesMapperAxrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private byte selectedBank;

    public NesMapperAxrom(
        Memory<byte> prg,
        Memory<byte> chr,
        NesCartridgeRamSizes ram) : base(ram)
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
            WriteChrRam(addr, value);
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return ReadChrRam(addr);

        return chr.Span[addr % chr.Length];
    }

}
