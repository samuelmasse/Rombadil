namespace Rombadil.Nes.Emulator;

public class NesMapperVrc2Vrc4 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly NesVrcRegisterMapping registerMapping;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] ram = new byte[0x2000];

    private byte prgBank0;
    private byte prgBank1;
    private readonly ushort[] chrBank = new ushort[8];
    private bool prgSwapMode;
    private bool ramEnable = true;

    private int irqLatch;
    private int irqCounter;
    private int irqPrescaler;
    private bool irqEnable;
    private bool irqEnableAfterAck;
    private bool irqCycleMode;

    public NesMapperVrc2Vrc4(
        Memory<byte> prg,
        Memory<byte> chr,
        NesMirroring mirroring,
        NesVrcRegisterMapping registerMapping)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;
        this.registerMapping = registerMapping;
    }

    public override byte Read(ushort addr)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF && ramEnable)
            return ram[(addr - 0x6000) & 0x1FFF];

        if (addr >= 0x8000)
            return ReadPrg(addr);

        return 0;
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x6000 && addr <= 0x7FFF)
        {
            if (ramEnable)
                ram[(addr - 0x6000) & 0x1FFF] = value;
            return;
        }

        if (addr < 0x8000)
            return;

        int reg = DecodeRegister(addr);
        switch (addr & 0xF000)
        {
            case 0x8000:
                prgBank0 = (byte)(value & 0x1F);
                break;

            case 0x9000:
                WriteControl(reg, value);
                break;

            case 0xA000:
                prgBank1 = (byte)(value & 0x1F);
                break;

            case >= 0xB000 and <= 0xE000:
                WriteChrRegister(addr, reg, value);
                break;

            case 0xF000:
                WriteIrqRegister(reg, value);
                break;
        }
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        int slot = (addr >> 10) & 7;
        int offset = addr & 0x03FF;
        return chr.Span[(chrBank[slot] * 0x400 + offset) % chr.Length];
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    public override void StepCpuCycle()
    {
        if (!irqEnable)
            return;

        if (irqCycleMode)
        {
            ClockIrqCounter();
            return;
        }

        irqPrescaler += 3;
        while (irqPrescaler >= 341)
        {
            irqPrescaler -= 341;
            ClockIrqCounter();
        }
    }

    private byte ReadPrg(ushort addr)
    {
        int bankCount = prg.Length / 0x2000;
        int bank = ((addr - 0x8000) >> 13) switch
        {
            0 => prgSwapMode ? bankCount - 2 : prgBank0,
            1 => prgBank1,
            2 => prgSwapMode ? prgBank0 : bankCount - 2,
            _ => bankCount - 1,
        };

        int offset = addr & 0x1FFF;
        return prg.Span[((bank % bankCount) * 0x2000 + offset) % prg.Length];
    }

    private void WriteControl(int reg, byte value)
    {
        if (reg == 0)
        {
            mirroring = (value & 0x03) switch
            {
                0 => NesMirroring.Vertical,
                1 => NesMirroring.Horizontal,
                2 => NesMirroring.SingleScreenLow,
                _ => NesMirroring.SingleScreenHigh,
            };
        }
        else if (reg == 2)
        {
            ramEnable = (value & 0x01) != 0;
            prgSwapMode = (value & 0x02) != 0;
        }
    }

    private void WriteChrRegister(ushort addr, int reg, byte value)
    {
        int pair = ((addr >> 12) - 0xB) * 2;
        int slot = pair + (reg >> 1);

        if (slot < 0 || slot >= chrBank.Length)
            return;

        if ((reg & 1) == 0)
            chrBank[slot] = (ushort)((chrBank[slot] & 0x1F0) | (value & 0x0F));
        else
            chrBank[slot] = (ushort)((chrBank[slot] & 0x00F) | ((value & 0x1F) << 4));
    }

    private void WriteIrqRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                irqLatch = (irqLatch & 0xF0) | (value & 0x0F);
                break;
            case 1:
                irqLatch = (irqLatch & 0x0F) | ((value & 0x0F) << 4);
                break;
            case 2:
                irqEnable = (value & 0x02) != 0;
                irqEnableAfterAck = (value & 0x01) != 0;
                irqCycleMode = (value & 0x04) != 0;
                if (irqEnable)
                {
                    irqCounter = irqLatch;
                    irqPrescaler = 0;
                }
                irqPending = false;
                break;
            case 3:
                irqEnable = irqEnableAfterAck;
                irqPending = false;
                break;
        }
    }

    private void ClockIrqCounter()
    {
        if (irqCounter == 0xFF)
        {
            irqCounter = irqLatch;
            irqPending = true;
        }
        else
        {
            irqCounter++;
        }
    }

    private int DecodeRegister(ushort addr) => registerMapping switch
    {
        NesVrcRegisterMapping.Mapper23Vrc2BOrVrc4F => DecodeMapper23Vrc2BOrVrc4FRegister(addr),
        NesVrcRegisterMapping.Mapper23Vrc4E => DecodeMapper23Vrc4ERegister(addr),
        NesVrcRegisterMapping.Mapper25Vrc2COrVrc4B => DecodeMapper25Vrc2COrVrc4BRegister(addr),
        NesVrcRegisterMapping.Mapper25Vrc4D => DecodeMapper25Vrc4DRegister(addr),
        NesVrcRegisterMapping.Mapper25 => DecodeMapper25Register(addr),
        _ => DecodeMapper23Register(addr),
    };

    private static int DecodeMapper23Register(ushort addr)
    {
        int vrc2B = DecodeMapper23Vrc2BOrVrc4FRegister(addr);
        int vrc4E = DecodeMapper23Vrc4ERegister(addr);
        return vrc2B != 0 ? vrc2B : vrc4E;
    }

    private static int DecodeMapper25Register(ushort addr)
    {
        int vrc2COrVrc4B = DecodeMapper25Vrc2COrVrc4BRegister(addr);
        int vrc4D = DecodeMapper25Vrc4DRegister(addr);
        return vrc2COrVrc4B != 0 ? vrc2COrVrc4B : vrc4D;
    }

    private static int DecodeMapper23Vrc2BOrVrc4FRegister(ushort addr)
    {
        int bit0 = addr & 0x01;
        int bit1 = (addr >> 1) & 0x01;
        return bit0 | (bit1 << 1);
    }

    private static int DecodeMapper23Vrc4ERegister(ushort addr)
    {
        int bit2 = (addr >> 2) & 0x01;
        int bit3 = (addr >> 3) & 0x01;
        return bit2 | (bit3 << 1);
    }

    private static int DecodeMapper25Vrc2COrVrc4BRegister(ushort addr)
    {
        int bit0 = addr & 0x01;
        int bit1 = (addr >> 1) & 0x01;
        return bit1 | (bit0 << 1);
    }

    private static int DecodeMapper25Vrc4DRegister(ushort addr)
    {
        int bit2 = (addr >> 2) & 0x01;
        int bit3 = (addr >> 3) & 0x01;
        return bit3 | (bit2 << 1);
    }
}
