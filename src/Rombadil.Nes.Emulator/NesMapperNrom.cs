namespace Rombadil.Nes.Emulator;

public class NesMapperNrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] prgRam;

    public NesMapperNrom(Memory<byte> prg, Memory<byte> chr, NesMirroring mirroring, int prgRamSize)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
        prgRam = new byte[prgRamSize];
    }

    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];
    public override void WritePrgRam(ushort addr, byte value)
    {
        if (prgRam.Length != 0)
            prgRam[(addr - 0x6000) % prgRam.Length] = value;
    }

    public override byte ReadPrgRam(ushort addr)
    {
        if (prgRam.Length == 0)
            return 0;

        return prgRam[(addr - 0x6000) % prgRam.Length];
    }

    public override byte ReadChr(ushort addr) => chr.Length == 0 ? chrRam[addr] : chr.Span[addr];
    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr] = value;
        else chr.Span[addr] = value;
    }
}
