namespace Rombadil.Nes.Emulator;

public class NesMmc5Pulse
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

    public int Length => length;

    public void Reset()
    {
        length = 0;
        enabled = false;
        halted = false;
        duty = 0;
        dutyStep = 0;
        timerPeriod = 0;
        timerCounter = 0;
        constantVolume = false;
        volumeOrEnvelope = 0;
        envelopeLoop = false;
        envelopeStart = false;
        envelopeDivider = 0;
        envelopeDecayLevel = 0;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                duty = (value & 0b1100_0000) >> 6;
                envelopeLoop = (value & 0b0010_0000) != 0;
                halted = envelopeLoop;
                constantVolume = (value & 0b0001_0000) != 0;
                volumeOrEnvelope = value & 0b0000_1111;
                break;

            case 1:
                break;

            case 2:
                timerPeriod = (timerPeriod & 0b111_0000_0000) | value;
                break;

            case 3:
                if (enabled)
                    length = NesApuLength.Table[(value & 0b1111_1000) >> 3];
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
            length--;
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
    }

    public float Sample()
    {
        if (length == 0)
            return 0;

        int vol = constantVolume ? volumeOrEnvelope : envelopeDecayLevel;
        return dutyTable[duty][dutyStep] != 0 ? vol : 0;
    }
}
