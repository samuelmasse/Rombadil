namespace Rombadil.Nes.Emulator;

public class NesApuPulse
{
    private static readonly byte[] lt =
    [
        10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
    ];

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
                halted = (value & 0x20) != 0;
                break;
            case 3:
                if (enabled)
                    length = lt[(value >> 3) & 0x1F];
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

    }

    public void ClockEnvelope()
    {

    }

    public void Step()
    {

    }
}
