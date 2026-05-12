namespace Rombadil.Nes.Emulator;

public class NesMapperMmc2 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] prgRam = new byte[0x2000];

    private byte prgBank;
    private byte chrBank0Fd;
    private byte chrBank0Fe;
    private byte chrBank1Fd;
    private byte chrBank1Fe;
    private bool latch0Fe;
    private bool latch1Fe;

    public NesMapperMmc2(Memory<byte> prg, Memory<byte> chr, NesMirroring mirroring)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
    }

    public override byte Read(ushort addr)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
            return ReadPrgRam(addr);

        if (addr >= 0x8000)
            return ReadPrgRom(addr);

        return 0;
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
        {
            WritePrgRam(addr, value);
            return;
        }

        if (addr >= 0x8000)
            WritePrgRom(addr, value);
    }

    public override byte ReadPrgRam(ushort addr) => prgRam[addr - 0x6000];
    public override void WritePrgRam(ushort addr, byte value) => prgRam[addr - 0x6000] = value;

    public override byte ReadPrgRom(ushort addr)
    {
        int bankCount = prg.Length / 0x2000;
        int window = (addr - 0x8000) >> 13;
        int bank = window switch
        {
            0 => prgBank,
            1 => bankCount - 3,
            2 => bankCount - 2,
            _ => bankCount - 1,
        };

        int offset = addr & 0x1FFF;
        return prg.Span[((bank % bankCount) * 0x2000 + offset) % prg.Length];
    }

    public override void WritePrgRom(ushort addr, byte value)
    {
        switch (addr & 0xF000)
        {
            case 0xA000: prgBank = (byte)(value & 0x0F); break;
            case 0xB000: chrBank0Fd = (byte)(value & 0x1F); break;
            case 0xC000: chrBank0Fe = (byte)(value & 0x1F); break;
            case 0xD000: chrBank1Fd = (byte)(value & 0x1F); break;
            case 0xE000: chrBank1Fe = (byte)(value & 0x1F); break;
            case 0xF000:
                mirroring = (value & 0x01) == 0 ? NesMirroring.Vertical : NesMirroring.Horizontal;
                break;
        }
    }

    public override byte ReadChr(ushort addr)
    {
        byte result;

        if (chr.Length == 0)
        {
            result = chrRam[addr & 0x1FFF];
        }
        else
        {
            int bank = addr < 0x1000
                ? latch0Fe ? chrBank0Fe : chrBank0Fd
                : latch1Fe ? chrBank1Fe : chrBank1Fd;

            int offset = addr & 0x0FFF;
            result = chr.Span[((bank * 0x1000) + offset) % chr.Length];
        }

        UpdateLatch(addr);
        return result;
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    private void UpdateLatch(ushort addr)
    {
        if (addr == 0x0FD8)
            latch0Fe = false;
        else if (addr == 0x0FE8)
            latch0Fe = true;
        else if (addr >= 0x1FD8 && addr <= 0x1FDF)
            latch1Fe = false;
        else if (addr >= 0x1FE8 && addr <= 0x1FEF)
            latch1Fe = true;
    }
}
