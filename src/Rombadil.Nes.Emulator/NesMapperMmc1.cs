namespace Rombadil.Nes.Emulator;

public class NesMapperMmc1 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] prgRam;
    private byte shift = 0x10;
    private byte control = 0x0C;
    private byte chrBank0;
    private byte chrBank1;
    private byte prgBank;
    private bool suppressSerialWrites;

    public NesMapperMmc1(Memory<byte> prg, Memory<byte> chr, int prgRamSize)
    {
        this.prg = prg;
        this.chr = chr;
        prgRam = new byte[prgRamSize];
        UpdateMirroring();
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
        {
            WritePrgRam(addr, value);
            return;
        }

        if (addr < 0x8000)
            return;

        WritePrgRom(addr, value);
    }

    public override void WritePrgRom(ushort addr, byte value)
    {
        if (suppressSerialWrites)
            return;

        suppressSerialWrites = true;

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
            return ReadPrgRam(addr);

        if (addr < 0x8000)
            return 0;

        return ReadPrgRom(addr);
    }

    public override byte ReadPrgRom(ushort addr)
    {
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

    public override void WritePrgRam(ushort addr, byte value)
    {
        if (prgRam.Length != 0 && !PrgRamDisabled)
            prgRam[(addr - 0x6000) % prgRam.Length] = value;
    }

    public override byte ReadPrgRam(ushort addr)
    {
        if (prgRam.Length == 0 || PrgRamDisabled)
            return 0;

        return prgRam[(addr - 0x6000) % prgRam.Length];
    }

    public override void StepCpuCycle() => suppressSerialWrites = false;

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
