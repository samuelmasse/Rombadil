namespace Rombadil.Nes.Emulator;

public class NesMapper148 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private byte selectedBank;

    public NesMapper148(
        Memory<byte> prg,
        Memory<byte> chr,
        NesMirroring mirroring,
        NesCartridgeRamSizes ram) : base(ram)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override byte Read(ushort addr)
    {
        if (addr >= 0x8000)
            return ReadPrgRom(addr);

        return 0;
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x8000)
            WritePrgRom(addr, value);
    }

    public override byte ReadPrgRom(ushort addr)
    {
        int bank = ((selectedBank >> 3) & 0x01) * 0x8000;
        return prg.Span[(bank + (addr - 0x8000)) % prg.Length];
    }

    public override void WritePrgRom(ushort addr, byte value)
    {
        selectedBank = (byte)(value & ReadPrgRom(addr));
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return ReadChrRam(addr);

        int bank = (selectedBank & 0x07) * 0x2000;
        return chr.Span[(bank + addr) % chr.Length];
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            WriteChrRam(addr, value);
    }
}
