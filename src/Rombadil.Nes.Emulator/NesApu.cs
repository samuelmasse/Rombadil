namespace Rombadil.Nes.Emulator;

public class NesApu
{
    private static readonly byte[] lt =
    [
        10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
    ];

    private NesApuChannel pulse1;
    private NesApuChannel pulse2;
    private NesApuChannel triangle;
    private NesApuChannel noise;
    private NesApuChannel dmc;
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
            case >= 0x4010 and <= 0x4013: WriteChannelRegister(ref dmc, addr - 0x4010, value); break;
            case 0x4015: WriteStatus(value); break;
            case 0x4017: WriteFrameCounter(value); break;
        }
    }

    public void Step()
    {
        int frame = frameFiveStep ? 18641 : 14915;

        if (frameCycle == 7457)
            ClockLength();
        else if (frameCycle == frame)
        {
            ClockLength();
            frameCycle = 0;

            if (!frameFiveStep && !frameIrqInhibit)
                frameIrq = true;
        }

        frameCycle++;
        cycles++;
    }

    private void WriteStatus(byte value)
    {
        ToggleChannel(ref pulse1, (value & 0b0000_0001) != 0);
        ToggleChannel(ref pulse2, (value & 0b0000_0010) != 0);
        ToggleChannel(ref triangle, (value & 0b0000_0100) != 0);
        ToggleChannel(ref noise, (value & 0b0000_1000) != 0);
        ToggleChannel(ref dmc, (value & 0b0001_0000) != 0);
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
