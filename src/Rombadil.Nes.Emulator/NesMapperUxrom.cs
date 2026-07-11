namespace Rombadil.Nes.Emulator;

public class NesMapperUxrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly bool busConflicts;
    private byte selectedBank;

    public NesMapperUxrom(
        Memory<byte> prg,
        Memory<byte> chr,
        NesMirroring mirroring,
        bool busConflicts,
        NesCartridgeRamSizes ram) : base(ram)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
        this.busConflicts = busConflicts;
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr < 0x8000)
            return;

        if (busConflicts)
            value &= Read(addr);

        selectedBank = (byte)(value & 0x0F);
    }

    public override byte Read(ushort addr)
    {
        if (addr < 0x8000)
            return 0;

        if (addr < 0xC000)
        {
            int bank = selectedBank * 0x4000;
            return prg.Span[(bank + (addr - 0x8000)) % prg.Length];
        }
        else
        {
            int bank = prg.Length - 0x4000;
            return prg.Span[bank + (addr - 0xC000)];
        }
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
