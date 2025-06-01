namespace Rombadil;

public class NesController
{
    private NesButtons currentState;
    private NesButtons latchedState;
    private int bitIndex;
    private bool strobe;

    public void SetButtons(NesButtons state) => currentState = state;

    public void Write(byte value)
    {
        bool newStrobe = (value & 1) != 0;
        if (strobe && !newStrobe)
        {
            latchedState = currentState;
            bitIndex = 0;
        }
        strobe = newStrobe;
    }

    public byte Read()
    {
        if (strobe)
        {
            latchedState = currentState;
            bitIndex = 0;
        }

        byte result = (byte)((((byte)latchedState >> bitIndex) & 1) | 0x40);
        if (bitIndex < 8)
            bitIndex++;

        return result;
    }
}
