namespace Rombadil.Nes.Emulator;

public class NesApuTriangle
{
    private int length;
    private bool enabled;
    private bool halted;

    private int timerPeriod;
    private int timerCounter;
    private byte linearReload;
    private bool linearControl;
    private bool linearReloadFlag;
    private int sequenceIndex;
    private int linearCounter;

    public int Length => length;

    public float Sample()
    {
        if (length == 0 || linearCounter == 0 || timerPeriod < 2)
            return 0;

        int value = sequenceIndex < 16 ? 15 - sequenceIndex : sequenceIndex - 16;
        return value;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                linearControl = (value & 0b1000_0000) != 0;
                linearReload = (byte)(value & 0b0111_1111);
                halted = linearControl;
                break;

            case 2:
                timerPeriod = (timerPeriod & 0x0700) | value;
                break;

            case 3:
                if (enabled)
                    length = NesApuLength.Table[(value >> 3) & 0b0001_1111];
                timerPeriod = (timerPeriod & 0x00FF) | ((value & 0x07) << 8);
                linearReloadFlag = true;
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

    public void ClockLinear()
    {
        if (linearReloadFlag)
            linearCounter = linearReload;
        else if (linearCounter > 0)
            linearCounter--;

        if (!linearControl)
            linearReloadFlag = false;
    }

    public void Step()
    {
        if (length == 0 || linearCounter == 0 || timerPeriod < 2)
            return;

        if (timerCounter == 0)
        {
            timerCounter = timerPeriod;
            sequenceIndex = (sequenceIndex + 1) & 0b0001_1111;
        }
        else timerCounter--;
    }
}
