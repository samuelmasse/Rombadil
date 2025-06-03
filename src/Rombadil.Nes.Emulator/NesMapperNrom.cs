namespace Rombadil.Nes.Emulator;

public class NesMapperNrom(Memory<byte> romPrg) : NesMapper
{
    public override byte Read(ushort addr) => romPrg.Span[(addr - 0x8000) % romPrg.Length];
}
