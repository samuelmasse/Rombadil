namespace Rombadil.Nes.Emulator;

public class NesApuNoise
{
    private int length;
    private bool enabled;
    private bool halted;

    private ushort shiftRegister = 1;
    private bool mode;
    private int timerPeriod;
    private int timerCounter;
    private bool constantVolume;
    private int volumeOrEnvelope;

    private bool envelopeLoop;
    private bool envelopeStart;
    private int envelopeDivider;
    private int envelopeDecayLevel;

    private static readonly int[] periodTable =
    [
        4, 8, 16, 32, 64, 96, 128, 160,
        202, 254, 380, 508, 762, 1016, 2034, 4068
    ];

    public int Length => length;

    public float Sample()
    {
        if (length == 0 || (shiftRegister & 1) == 1)
            return 0;

        int volume = constantVolume ? volumeOrEnvelope : envelopeDecayLevel;
        return volume;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                envelopeLoop = (value & 0b0010_0000) != 0;
                constantVolume = (value & 0b0001_0000) != 0;
                volumeOrEnvelope = value & 0b0000_1111;
                halted = envelopeLoop;
                break;

            case 2:
                mode = (value & 0b1000_0000) != 0;
                timerPeriod = periodTable[value & 0b0000_1111] / 2 - 1;
                break;

            case 3:
                if (enabled)
                    length = NesApuLength.Table[(value >> 3) & 0b0001_1111];
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

            int bit0 = shiftRegister & 1;
            int bitX = (shiftRegister >> (mode ? 6 : 1)) & 1;
            int feedback = bit0 ^ bitX;

            shiftRegister >>= 1;
            shiftRegister |= (ushort)(feedback << 14);
        }
        else timerCounter--;
    }
}
