namespace Rombadil.Nes.Emulator;

public class NesMapperNrom(Memory<byte> prg, Memory<byte> chr) : NesMapper
{
    public override byte Read(ushort addr) => prg.Span[(addr - 0x8000) % prg.Length];
    public override byte ReadChr(ushort addr) => chr.Span[addr];
}
