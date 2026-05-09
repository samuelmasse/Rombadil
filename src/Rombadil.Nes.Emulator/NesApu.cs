namespace Rombadil.Nes.Emulator;

public class NesApu(NesMapper mapper, List<int> samples)
{
    private readonly NesApuPulse pulse1 = new(false);
    private readonly NesApuPulse pulse2 = new(true);
    private readonly NesApuTriangle triangle = new();
    private readonly NesApuNoise noise = new();
    private readonly NesApuDmc dmc = new(mapper);

    private bool frameIrq;
    private bool frameFiveStep;
    private bool frameIrqInhibit;
    private int frameCycle;
    private int frameIrqAssertCycles;
    private long cycles;

    public long Cycles => cycles;

    public void Reset() => cycles = 0;

    public byte ReadStatus()
    {
        var status = PeekStatus();
        frameIrq = false;
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
        if (dmc.IrqFlag) status |= 0b1000_0000;

        return status;
    }

    public void WriteRegister(ushort addr, byte value)
    {
        switch (addr)
        {
            case >= 0x4000 and <= 0x4003: pulse1.WriteRegister(addr - 0x4000, value); break;
            case >= 0x4004 and <= 0x4007: pulse2.WriteRegister(addr - 0x4004, value); break;
            case >= 0x4008 and <= 0x400B: triangle.WriteRegister(addr - 0x4008, value); break;
            case >= 0x400C and <= 0x400F: noise.WriteRegister(addr - 0x400C, value); break;
            case >= 0x4010 and <= 0x4013: dmc.WriteRegister(addr - 0x4010, value); break;
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
                frameIrqAssertCycles = 3;

            frameCycle++;
        }
        else
        {
            if (frameCycle == 3730)
                ClockEnvelopes();
            else if (frameCycle == 7458)
            {
                ClockEnvelopes();
                ClockLengthAndSweep();
            }
            else if (frameCycle == 11187)
                ClockEnvelopes();
            else if (frameCycle == frame + 1)
            {
                ClockEnvelopes();
                ClockLengthAndSweep();
                frameCycle -= frame;
            }

            pulse1.Step();
            pulse2.Step();
            noise.Step();
        }

        if (frameIrqAssertCycles > 0)
        {
            SetFrameInterruptIfRequired();
            frameIrqAssertCycles--;
        }

        triangle.Step();
        dmc.Step();

        samples.Add(Sample());
        cycles++;
    }

    private int Sample()
    {
        float p1 = pulse1.Sample();
        float p2 = pulse2.Sample();
        float tri = triangle.Sample();
        float noi = noise.Sample();
        float dm = dmc.Sample();

        float pulseMix = p1 + p2;
        float pulseOut = pulseMix == 0
            ? 0
            : 95.88f / ((8128f / pulseMix) + 100f);

        float tndMix = tri / 8227f + noi / 12241f + dm / 22638f;
        float tndOut = tndMix == 0
            ? 0
            : 159.79f / ((1f / tndMix) + 100f);

        float output = pulseOut + tndOut;
        return (int)(Math.Clamp(output, 0f, 1f) * short.MaxValue);
    }

    private void SetFrameInterruptIfRequired()
    {
        if (!frameFiveStep && !frameIrqInhibit)
            frameIrq = true;
    }

    private void WriteStatus(byte value)
    {
        pulse1.Toggle((value & 0b0000_0001) != 0);
        pulse2.Toggle((value & 0b0000_0010) != 0);
        triangle.Toggle((value & 0b0000_0100) != 0);
        noise.Toggle((value & 0b0000_1000) != 0);
        dmc.Toggle((value & 0b0001_0000) != 0);
    }

    private void WriteFrameCounter(byte value)
    {
        frameFiveStep = (value & 0x80) != 0;
        frameIrqInhibit = (value & 0x40) != 0;
        frameCycle = 0;
        frameIrqAssertCycles = 0;

        if (frameIrqInhibit)
            frameIrq = false;

        if (frameFiveStep)
        {
            ClockLengthAndSweep();
            ClockEnvelopes();
        }
    }

    private void ClockLengthAndSweep()
    {
        pulse1.ClockLength();
        pulse2.ClockLength();
        triangle.ClockLength();
        noise.ClockLength();

        pulse1.ClockSweep();
        pulse2.ClockSweep();
    }

    private void ClockEnvelopes()
    {
        pulse1.ClockEnvelope();
        pulse2.ClockEnvelope();
        noise.ClockEnvelope();
        triangle.ClockLinear();
    }
}
