namespace Rombadil.Nes.Emulator;

public class NesApuDmc(NesMapper mapper)
{
    private static readonly int[] rates =
    [
        428, 380, 340, 320, 286, 254, 226, 214,
        190, 160, 142, 128, 106,  84,  72,  54
    ];

    private bool enabled;
    private bool irqEnable;
    private bool loop;
    private byte rateIndex;
    private ushort sampleAddress;
    private int sampleLength;
    private ushort currentAddress;
    private int sampleRemaining;
    private bool irqFlag;
    private bool bufferFilled;
    private byte sampleBuffer;
    private byte shiftRegister;
    private int bitCounter;
    private byte outputLevel;
    private int timer;

    public bool Enabled => enabled;
    public bool IrqFlag => irqFlag;

    public void Step()
    {
        if (timer > 0)
            timer--;

        if (timer == 0)
        {
            ClockDmcOutput();
            timer = rates[rateIndex];
        }
    }

    public void Toggle(bool enable)
    {
        enabled = enable;

        if (enable)
            StartDmcSample();
        else StopDmcSample();

        irqFlag = false;
    }

    public void WriteRegister(int reg, byte value)
    {
        switch (reg)
        {
            case 0:
                irqEnable = (value & 0b1000_0000) != 0;
                loop = (value & 0b0100_0000) != 0;
                rateIndex = (byte)(value & 0b0000_1111);
                if (!irqEnable)
                    irqFlag = false;
                break;

            case 1:
                outputLevel = (byte)(value & 0b0111_1111);
                break;

            case 2:
                sampleAddress = (ushort)(0xC000 + (value << 6));
                break;

            case 3:
                sampleLength = (value << 4) + 1;
                break;
        }
    }

    private void ClockDmcOutput()
    {
        if (bitCounter == 0)
        {
            if (!bufferFilled)
            {
                if (sampleRemaining == 0 && !loop)
                    enabled = false;
            }

            shiftRegister = sampleBuffer;
            bitCounter = 8;
            bufferFilled = false;

            FetchDmcSample();
        }

        if ((shiftRegister & 1) != 0)
        {
            if (outputLevel <= 125) outputLevel += 2;
        }
        else
        {
            if (outputLevel >= 2) outputLevel -= 2;
        }

        shiftRegister >>= 1;
        bitCounter--;
    }

    private void StartDmcSample()
    {
        if (sampleRemaining == 0)
        {
            currentAddress = sampleAddress;
            sampleRemaining = sampleLength;
        }

        if (!bufferFilled)
            FetchDmcSample();

        if (timer <= 0)
            timer = rates[rateIndex];
    }

    private void StopDmcSample()
    {
        sampleRemaining = 0;
    }

    private void FetchDmcSample()
    {
        if (sampleRemaining <= 0)
            return;

        sampleBuffer = mapper.Read(currentAddress);
        bufferFilled = true;

        currentAddress++;
        if (currentAddress == 0x0000)
            currentAddress = 0x8000;

        sampleRemaining--;

        if (sampleRemaining != 0)
            return;

        if (loop)
        {
            currentAddress = sampleAddress;
            sampleRemaining = sampleLength;
        }
        else
        {
            if (irqEnable)
                irqFlag = true;

            enabled = false;
        }
    }
}
