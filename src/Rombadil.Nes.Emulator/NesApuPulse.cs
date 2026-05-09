namespace Rombadil.Nes.Emulator;

public class NesApuPulse(bool isSecondChannel)
{
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

    private bool pendingHalt;
    private bool pendingHaltValue;
    private bool pendingReload;
    private byte pendingReloadValue;
    private bool lengthClockedThisStep;
    private bool lengthWasNonZeroBeforeClock;

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
        if (length == 0 || timerPeriod < 8 || timerPeriod > 0b0111_1111_1111)
            return 0;

        int vol = constantVolume ? volumeOrEnvelope : envelopeDecayLevel;
        int output = dutyTable[duty][dutyStep] != 0 ? vol : 0;

        return output;
    }

    public void WriteRegister(int reg, byte value, bool atLengthClockCycle)
    {
        switch (reg)
        {
            case 0:
                duty = (value & 0b1100_0000) >> 6;
                envelopeLoop = (value & 0b0010_0000) != 0;
                constantVolume = (value & 0b0001_0000) != 0;
                volumeOrEnvelope = value & 0b0000_1111;
                pendingHalt = true;
                pendingHaltValue = envelopeLoop;
                break;

            case 1:
                sweepEnabled = (value & 0b1000_0000) != 0;
                sweepPeriod = (value & 0b0111_0000) >> 4;
                sweepNegate = (value & 0b0000_1000) != 0;
                sweepShift = value & 0b0000_0111;
                sweepReload = true;
                break;

            case 2:
                timerPeriod = (timerPeriod & 0b111_0000_0000) | (value & 0b0000_1111_1111);
                break;

            case 3:
                if (enabled)
                {
                    if (atLengthClockCycle)
                    {
                        pendingReload = true;
                        pendingReloadValue = value;
                    }
                    else length = NesApuLength.Table[(value & 0b1111_1000) >> 3];
                }
                timerPeriod = (timerPeriod & 0b0000_1111_1111) | ((value & 0b0000_0111) << 8);
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
        {
            length--;
            lengthWasNonZeroBeforeClock = true;
        }
        lengthClockedThisStep = true;
    }

    public void EndOfCycle()
    {
        if (pendingHalt)
        {
            halted = pendingHaltValue;
            pendingHalt = false;
        }
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

                if (target >= 8 && target <= 0b0111_1111_1111)
                    timerPeriod = target;
            }
        }
    }

    public void ClockEnvelope()
    {
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
            dutyStep = (dutyStep + 1) & 0b0111;
        }
        else timerCounter--;

        if (pendingReload)
        {
            if (!(lengthClockedThisStep && lengthWasNonZeroBeforeClock))
                length = NesApuLength.Table[(pendingReloadValue & 0b1111_1000) >> 3];
            pendingReload = false;
        }

        lengthClockedThisStep = false;
        lengthWasNonZeroBeforeClock = false;
    }
}
