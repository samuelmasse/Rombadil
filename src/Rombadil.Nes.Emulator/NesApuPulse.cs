namespace Rombadil.Nes.Emulator;

public class NesApuPulse(bool isSecondChannel)
{
    private static readonly byte[] lt =
    [
        10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
    ];

    private static readonly byte[][] dutyTable =
    [
        [0, 1, 0, 0, 0, 0, 0, 0],
        [0, 1, 1, 0, 0, 0, 0, 0],
        [0, 1, 1, 1, 1, 0, 0, 0],
        [1, 0, 0, 1, 1, 1, 1, 1],
    ];

    private int length;
    private bool enabled;
    private bool halted;

    private int duty;
    private int dutyStep;
    private int timerPeriod;
    private int timerCounter;
    private bool constantVolume;
    private int volumeOrEnvelope;

    private bool envelopeLoop;
    private bool envelopeStart;
    private int envelopeDivider;
    private int envelopeDecayLevel;

    private bool sweepEnabled;
    private int sweepPeriod;
    private bool sweepNegate;
    private int sweepShift;
    private int sweepDivider;
    private bool sweepReload;

    public int Length => length;

    public float Sample()
    {
        if (length == 0 || timerPeriod < 8 || timerPeriod > 0x7FF)
            return 0;

        int vol = constantVolume ? volumeOrEnvelope : envelopeDecayLevel;
        int output = dutyTable[duty][dutyStep] != 0 ? vol : 0;

        return output;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                duty = (value >> 6) & 0b11;
                envelopeLoop = (value & 0x20) != 0;
                constantVolume = (value & 0x10) != 0;
                volumeOrEnvelope = value & 0x0F;
                halted = envelopeLoop;
                break;

            case 1:
                sweepEnabled = (value & 0x80) != 0;
                sweepPeriod = (value >> 4) & 0b111;
                sweepNegate = (value & 0x08) != 0;
                sweepShift = value & 0b111;
                sweepReload = true;
                break;

            case 2:
                timerPeriod = (timerPeriod & 0x700) | value;
                break;

            case 3:
                if (enabled)
                    length = lt[(value >> 3) & 0x1F];
                timerPeriod = (timerPeriod & 0xFF) | ((value & 0x07) << 8);
                dutyStep = 0;
                envelopeStart = true;
                break;
        }
    }

    public void Toggle(bool on)
    {
        enabled = on;
        if (!on)
            length = 0;
    }

    public void ClockLength()
    {
        if (!halted && length > 0)
            length--;
    }

    public void ClockSweep()
    {
        if (sweepReload)
        {
            sweepDivider = sweepPeriod;
            sweepReload = false;
        }
        else if (--sweepDivider <= 0)
        {
            sweepDivider = sweepPeriod;
            if (sweepEnabled && sweepShift > 0)
            {
                int delta = timerPeriod >> sweepShift;
                int target = sweepNegate
                    ? timerPeriod - (isSecondChannel ? delta + 1 : delta)
                    : timerPeriod + delta;

                if (target >= 8 && target <= 0x7FF)
                    timerPeriod = target;
            }
        }
    }

    public void ClockEnvelope()
    {
        Console.WriteLine($"ENV tick: start={envelopeStart} decay={envelopeDecayLevel}, divider={envelopeDivider}, loop={envelopeLoop}");

        if (envelopeStart)
        {
            envelopeStart = false;
            envelopeDecayLevel = 15;
            envelopeDivider = volumeOrEnvelope;
        }
        else
        {
            if (envelopeDivider == 0)
            {
                envelopeDivider = volumeOrEnvelope;

                if (envelopeDecayLevel > 0)
                    envelopeDecayLevel--;
                else if (envelopeLoop)
                    envelopeDecayLevel = 15;
            }
            else
            {
                envelopeDivider--;
            }
        }
    }

    public void Step()
    {
        if (timerCounter == 0)
        {
            timerCounter = timerPeriod;
            dutyStep = (dutyStep + 1) & 7;
        }
        else timerCounter--;
    }
}
