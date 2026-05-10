namespace Rombadil.Nes.Emulator;

public class NesMapperMmc3 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly bool fourScreen;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] ram = new byte[0x2000];

    private readonly byte[] bankRegs = new byte[8];
    private byte bankSelect;
    private bool prgMode;
    private bool chrMode;

    private byte mirror;
    private bool ramEnable;

    private byte irqLatch;
    private byte irqCounter;
    private bool irqReload;
    private bool irqEnable;

    public NesMapperMmc3(Memory<byte> prg, Memory<byte> chr, bool fourScreen)
    {
        this.prg = prg;
        this.chr = chr;
        this.fourScreen = fourScreen;
        UpdateMirroring();
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x8000 && addr <= 0x9FFF)
        {
            if ((addr & 1) == 0)
            {
                bankSelect = value;
                prgMode = (value & 0x40) != 0;
                chrMode = (value & 0x80) != 0;
            }
            else
            {
                int bank = bankSelect & 0x07;
                bankRegs[bank] = bank >= 6 ? (byte)(value & 0x3F) : value;
            }
        }
        else if (addr >= 0xA000 && addr <= 0xBFFF)
        {
            if ((addr & 1) == 0)
            {
                mirror = (byte)(value & 1);
                UpdateMirroring();
            }
            else ramEnable = (value & 0x80) == 0;
        }
        else if (addr >= 0x6000 && addr <= 0x7FFF && ramEnable)
        {
            ram[addr - 0x6000] = value;
        }
        else if (addr >= 0xC000 && addr <= 0xDFFF)
        {
            if ((addr & 1) == 0)
            {
                irqLatch = value;
            }
            else
            {
                irqCounter = 0;
                irqReload = true;
            }
        }
        else if (addr >= 0xE000 && addr <= 0xFFFF)
        {
            if ((addr & 1) == 0)
            {
                irqEnable = false;
                irqPending = false;
            }
            else
            {
                irqEnable = true;
            }
        }
    }

    public override byte Read(ushort addr)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF && ramEnable)
            return ram[addr - 0x6000];

        if (addr >= 0x8000)
        {
            int bank = (addr - 0x8000) / 0x2000;
            int offset = addr & 0x1FFF;

            int mappedBank = bank switch
            {
                0 => prgMode ? prg.Length / 0x2000 - 2 : bankRegs[6],
                1 => bankRegs[7],
                2 => prgMode ? bankRegs[6] : prg.Length / 0x2000 - 2,
                3 => prg.Length / 0x2000 - 1,
                _ => 0
            };

            int mappedAddr = (mappedBank * 0x2000 + offset) % prg.Length;
            return prg.Span[mappedAddr];
        }

        return 0;
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        int bank;
        int offset;

        if (chrMode)
        {
            if (addr < 0x0800)
            {
                bank = 2 + (addr / 0x0400);
                offset = addr & 0x03FF;
            }
            else if (addr < 0x1000)
            {
                bank = 4 + ((addr - 0x0800) / 0x0400);
                offset = addr & 0x03FF;
            }
            else if (addr < 0x1800)
            {
                bank = 0;
                offset = addr - 0x1000;
            }
            else
            {
                bank = 1;
                offset = addr - 0x1800;
            }
        }
        else
        {
            if (addr < 0x0800)
            {
                bank = 0;
                offset = addr;
            }
            else if (addr < 0x1000)
            {
                bank = 1;
                offset = addr - 0x0800;
            }
            else if (addr < 0x1400)
            {
                bank = 2;
                offset = addr - 0x1000;
            }
            else if (addr < 0x1800)
            {
                bank = 3;
                offset = addr - 0x1400;
            }
            else if (addr < 0x1C00)
            {
                bank = 4;
                offset = addr - 0x1800;
            }
            else
            {
                bank = 5;
                offset = addr - 0x1C00;
            }
        }

        int regValue = bank < 2 ? bankRegs[bank] & 0xFE : bankRegs[bank];
        return chr.Span[(regValue * 0x400 + offset) % chr.Length];
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    private void UpdateMirroring()
    {
        if (fourScreen)
        {
            mirroring = NesMirroring.FourScreen;
            return;
        }

        mirroring = mirror == 0 ? NesMirroring.Vertical : NesMirroring.Horizontal;
    }

    public override void ClockIrq()
    {
        if (irqCounter == 0 || irqReload)
        {
            irqCounter = irqLatch;
            irqReload = false;
        }
        else
        {
            irqCounter--;
            if (irqCounter == 0 && irqEnable)
                irqPending = true;
        }
    }
}
