namespace Rombadil.Nes.Emulator;

public class NesMapperNrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];

    public NesMapperNrom(Memory<byte> prg, Memory<byte> chr, NesMirroring mirroring)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];
    public override byte ReadChr(ushort addr) => chr.Length == 0 ? chrRam[addr] : chr.Span[addr];
    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr] = value;
        else chr.Span[addr] = value;
    }
}
