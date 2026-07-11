namespace Rombadil.Nes.Emulator;

public class NesMapperCnrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private byte selectedBank;

    public NesMapperCnrom(
        Memory<byte> prg,
        Memory<byte> chr,
        NesMirroring mirroring,
        NesCartridgeRamSizes ram) : base(ram)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override void Write(ushort addr, byte value) => selectedBank = value;

    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return ReadChrRam(addr);

        int bank = selectedBank * 0x2000;
        return chr.Span[(bank + addr) % chr.Length];
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            WriteChrRam(addr, value);
    }
}
