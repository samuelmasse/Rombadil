namespace Rombadil.Nes.Emulator;

public class NesMapperNrom(Memory<byte> prg, Memory<byte> chr) : NesMapper
{
    private readonly byte[] chrRam = new byte[0x2000];

    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];
    public override byte ReadChr(ushort addr) => chr.Length == 0 ? chrRam[addr] : chr.Span[addr];
    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr] = value;
        else chr.Span[addr] = value;
    }
}
