namespace Rombadil.Nes.Emulator;

public class NesMapperNrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;

    public NesMapperNrom(
        Memory<byte> prg,
        Memory<byte> chr,
        NesMirroring mirroring,
        NesCartridgeRamSizes ram) : base(ram)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];

    public override byte ReadChr(ushort addr) => chr.Length == 0 ? ReadChrRam(addr) : chr.Span[addr];
    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            WriteChrRam(addr, value);
    }
}
