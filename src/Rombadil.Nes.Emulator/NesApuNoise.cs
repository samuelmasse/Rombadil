namespace Rombadil.Nes.Emulator;

public class NesApuNoise
{
    private int length;
    private bool enabled;
    private bool halted;

    public int Length => length;

    public float Sample()
    {
        return 0;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                halted = (value & 0b0010_0000) != 0;
                break;

            case 3:
                if (enabled)
                    length = NesApuLength.Table[(value >> 3) & 0b0001_1111];
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

    }

    public void Step()
    {

    }
}
