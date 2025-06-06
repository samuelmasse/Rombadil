namespace Rombadil.Nes.Emulator;

public class NesApu(NesMapper mapper)
{
    private static readonly byte[] lt =
    [
        10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
    ];

    private static readonly int[] dmcRates =
    [
        428, 380, 340, 320, 286, 254, 226, 214,
        190, 160, 142, 128, 106,  84,  72,  54
    ];

    private NesApuChannel pulse1;
    private NesApuChannel pulse2;
    private NesApuChannel triangle;
    private NesApuChannel noise;
    private NesApuChannel dmc;

    private bool dmcIrqEnable;
    private bool dmcLoop;
    private byte dmcRateIndex;
    private ushort dmcSampleAddress;
    private int dmcSampleLength;
    private ushort dmcCurrentAddress;
    private int dmcSampleRemaining;
    private bool dmcIrqFlag;
    private bool dmcBufferFilled;
    private byte dmcSampleBuffer;
    private byte dmcShiftRegister;
    private int dmcBitCounter;
    private byte dmcOutputLevel;
    private int dmcTimer;

    private bool frameIrq;
    private bool frameFiveStep;
    private bool frameIrqInhibit;
    private int frameCycle;
    private long cycles;

    public long Cycles => cycles;

    public byte ReadStatus()
    {
        var status = PeekStatus();
        frameIrq = false;
        Console.WriteLine($"APU CYC:{cycles} READ ${status:X2}");
        return status;
    }

    public byte PeekStatus()
    {
        byte status = 0;

        if (pulse1.Length > 0) status |= 0b0000_0001;
        if (pulse2.Length > 0) status |= 0b0000_0010;
        if (triangle.Length > 0) status |= 0b0000_0100;
        if (noise.Length > 0) status |= 0b0000_1000;
        if (dmc.Enabled) status |= 0b0001_0000;
        if (frameIrq) status |= 0b0100_0000;
        if (dmcIrqFlag) status |= 0b1000_0000;

        return status;
    }

    public void WriteRegister(ushort addr, byte value)
    {
        Console.WriteLine($"APU CYC:{cycles} WRITE ${addr:X4}=${value:X2}");

        switch (addr)
        {
            case >= 0x4000 and <= 0x4003: WriteChannelRegister(ref pulse1, addr - 0x4000, value); break;
            case >= 0x4004 and <= 0x4007: WriteChannelRegister(ref pulse2, addr - 0x4004, value); break;
            case >= 0x4008 and <= 0x400B: WriteChannelRegister(ref triangle, addr - 0x4008, value); break;
            case >= 0x400C and <= 0x400F: WriteChannelRegister(ref noise, addr - 0x400C, value); break;
            case >= 0x4010 and <= 0x4013: WriteDmcRegister(addr - 0x4010, value); break;
            case 0x4015: WriteStatus(value); break;
            case 0x4017: WriteFrameCounter(value); break;
        }
    }

    public void Step()
    {
        int frame = frameFiveStep ? 18641 : 14915;

        if (cycles % 2 == 0)
        {
            if (frameCycle == frame)
                SetFrameInterruptIfRequired();

            frameCycle++;
        }
        else
        {
            if (frameCycle == 7458)
                ClockLength();
            if (frameCycle == frame + 1)
            {
                ClockLength();
                frameCycle -= frame;
            }
        }

        cycles++;

        if (dmcTimer > 0)
            dmcTimer--;

        if (dmcTimer == 0)
        {
            ClockDmcOutput();
            dmcTimer = dmcRates[dmcRateIndex];
        }
    }

    private void ClockDmcOutput()
    {
        if (dmcBitCounter == 0)
        {
            if (!dmcBufferFilled)
                return;

            dmcShiftRegister = dmcSampleBuffer;
            dmcBitCounter = 8;
            dmcBufferFilled = false;

            FetchDmcSample();
        }

        if ((dmcShiftRegister & 1) != 0)
        {
            if (dmcOutputLevel <= 125) dmcOutputLevel += 2;
        }
        else
        {
            if (dmcOutputLevel >= 2) dmcOutputLevel -= 2;
        }

        dmcShiftRegister >>= 1;
        dmcBitCounter--;
    }

    private void SetFrameInterruptIfRequired()
    {
        if (!frameFiveStep && !frameIrqInhibit)
            frameIrq = true;
    }

    private void WriteStatus(byte value)
    {
        ToggleChannel(ref pulse1, (value & 0b0000_0001) != 0);
        ToggleChannel(ref pulse2, (value & 0b0000_0010) != 0);
        ToggleChannel(ref triangle, (value & 0b0000_0100) != 0);
        ToggleChannel(ref noise, (value & 0b0000_1000) != 0);

        bool dmcEnable = (value & 0b0001_0000) != 0;
        ToggleChannel(ref dmc, dmcEnable);

        if (dmcEnable)
            StartDmcSample();
        else StopDmcSample();

        dmcIrqFlag = false;
    }

    private void StartDmcSample()
    {
        if (dmcSampleRemaining == 0)
        {
            dmcCurrentAddress = dmcSampleAddress;
            dmcSampleRemaining = dmcSampleLength;
        }

        if (!dmcBufferFilled)
            FetchDmcSample();

        if (dmcTimer <= 0)
            dmcTimer = dmcRates[dmcRateIndex];
    }

    private void StopDmcSample()
    {
        dmcSampleRemaining = 0;
    }

    private void FetchDmcSample()
    {
        if (dmcSampleRemaining <= 0)
            return;

        dmcSampleBuffer = mapper.Read(dmcCurrentAddress);
        dmcBufferFilled = true;

        dmcCurrentAddress++;
        if (dmcCurrentAddress == 0x0000)
            dmcCurrentAddress = 0x8000;

        dmcSampleRemaining--;

        if (dmcSampleRemaining != 0)
            return;

        if (dmcLoop)
        {
            dmcCurrentAddress = dmcSampleAddress;
            dmcSampleRemaining = dmcSampleLength;
        }
        else
        {
            if (dmcIrqEnable)
                dmcIrqFlag = true;

            dmc.Enabled = false;
        }
    }

    private void WriteFrameCounter(byte value)
    {
        frameFiveStep = (value & 0x80) != 0;
        frameIrqInhibit = (value & 0x40) != 0;
        frameCycle = 0;

        if (frameIrqInhibit)
            frameIrq = false;

        if (frameFiveStep)
            ClockLength();
    }

    private void ClockLength()
    {
        ClockLengthChannel(ref pulse1);
        ClockLengthChannel(ref pulse2);
        ClockLengthChannel(ref triangle);
        ClockLengthChannel(ref noise);
    }

    private void WriteDmcRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                dmcIrqEnable = (value & 0b1000_0000) != 0;
                dmcLoop = (value & 0b0100_0000) != 0;
                dmcRateIndex = (byte)(value & 0b0000_1111);
                if (!dmcIrqEnable)
                    dmcIrqFlag = false;
                break;

            case 1:
                dmcOutputLevel = (byte)(value & 0b0111_1111);
                break;

            case 2:
                dmcSampleAddress = (ushort)(0xC000 + (value << 6));
                break;

            case 3:
                dmcSampleLength = (value << 4) + 1;
                break;
        }

        WriteChannelRegister(ref dmc, reg, value);
    }

    private void WriteChannelRegister(ref NesApuChannel channel, int reg, byte value)
    {
        if (reg == 0)
            channel.Halted = (value & 0x20) != 0;
        else if (reg == 3 && channel.Enabled)
            channel.Length = lt[(value >> 3) & 0x1F];
    }

    private void ToggleChannel(ref NesApuChannel channel, bool enabled)
    {
        channel.Enabled = enabled;
        if (!enabled)
            channel.Length = 0;
    }

    private void ClockLengthChannel(ref NesApuChannel channel)
    {
        if (!channel.Halted && channel.Length > 0)
            channel.Length--;
    }
}

public struct NesApuChannel
{
    public int Length;
    public bool Enabled;
    public bool Halted;
}
