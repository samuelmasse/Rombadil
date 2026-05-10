namespace Rombadil.Nes.Emulator;

public class NesMapperUxrom : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private byte selectedBank;

    public NesMapperUxrom(Memory<byte> prg, Memory<byte> chr, NesMirroring mirroring)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override void Write(ushort addr, byte value) => selectedBank = (byte)(value & 0x0F);

    public override byte Read(ushort addr)
    {
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
            chrRam[addr & 0x1FFF] = value;
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        return chr.Span[addr % chr.Length];
    }
}
