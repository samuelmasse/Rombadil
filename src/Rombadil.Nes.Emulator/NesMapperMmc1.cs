namespace Rombadil.Nes.Emulator;

public class NesMapperMmc1 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] prgRam = new byte[0x2000];
    private byte shift = 0x10;
    private byte control = 0x0C;
    private byte chrBank0;
    private byte chrBank1;
    private byte prgBank;

    public NesMapperMmc1(Memory<byte> prg, Memory<byte> chr)
    {
        this.prg = prg;
        this.chr = chr;
        UpdateMirroring();
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
        {
            if (!PrgRamDisabled)
                prgRam[addr - 0x6000] = value;
            return;
        }

        if ((value & 0x80) != 0)
        {
            shift = 0x10;
            control |= 0x0C;
            UpdateMirroring();
            return;
        }

        bool complete = (shift & 1) != 0;
        shift = (byte)((shift >> 1) | ((value & 1) << 4));

        if (complete)
        {
            int reg = (addr >> 13) & 0b11;

            switch (reg)
            {
                case 0: control = shift; UpdateMirroring(); break;
                case 1: chrBank0 = shift; break;
                case 2: chrBank1 = shift; break;
                case 3: prgBank = shift; break;
            }

            shift = 0x10;
        }
    }

    public override byte Read(ushort addr)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
            return PrgRamDisabled ? (byte)0 : prgRam[addr - 0x6000];

        int mode = (control >> 2) & 0b11;
        int prgSelect = prgBank & 0x0F;
        int prgBase = PrgBaseOffset;

        if (mode is 0 or 1)
        {
            int bank = (prgSelect & 0x0E) * 0x4000;
            int index = prgBase + bank + (addr - 0x8000);
            return prg.Span[index % prg.Length];
        }
        else if (mode == 2)
        {
            if (addr < 0xC000)
                return prg.Span[(prgBase + (addr - 0x8000)) % prg.Length];

            int bank = prgSelect * 0x4000;
            int index = prgBase + bank + (addr - 0xC000);
            return prg.Span[index % prg.Length];
        }
        else
        {
            if (addr < 0xC000)
            {
                int bank = prgSelect * 0x4000;
                int index = prgBase + bank + (addr - 0x8000);
                return prg.Span[index % prg.Length];
            }

            int lastBank = (prgBase + 0x40000 - 0x4000) % prg.Length;
            return prg.Span[lastBank + (addr - 0xC000)];
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

        if ((control & 0x10) != 0)
        {
            if (addr < 0x1000)
            {
                int bank = chrBank0 * 0x1000;
                return chr.Span[(bank + addr) % chr.Length];
            }
            else
            {
                int bank = chrBank1 * 0x1000;
                return chr.Span[(bank + (addr - 0x1000)) % chr.Length];
            }
        }
        else
        {
            int bank = (chrBank0 & 0xFE) * 0x1000;
            return chr.Span[(bank + addr) % chr.Length];
        }
    }

    private void UpdateMirroring()
    {
        mirroring = (control & 0b11) switch
        {
            0 => NesMirroring.SingleScreenLow,
            1 => NesMirroring.SingleScreenHigh,
            2 => NesMirroring.Vertical,
            _ => NesMirroring.Horizontal,
        };
    }

    private bool PrgRamDisabled => (prgBank & 0x10) != 0;

    private int PrgBaseOffset
    {
        get
        {
            if (prg.Length <= 0x40000)
                return 0;

            return ((chrBank0 >> 4) & 1) * 0x40000;
        }
    }
}
