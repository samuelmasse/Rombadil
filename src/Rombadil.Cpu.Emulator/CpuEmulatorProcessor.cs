namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorProcessor(CpuEmulatorState s)
{
    internal byte AC
    {
        get => s.AC;
        set
        {
            s.AC = value;
            SetZN(value);
        }
    }

    internal byte X
    {
        get => s.X;
        set
        {
            s.X = value;
            SetZN(value);
        }
    }

    internal byte Y
    {
        get => s.Y;
        set
        {
            s.Y = value;
            SetZN(value);
        }
    }

    internal void SetZN(byte value)
    {
        s.Zero = value == 0;
        s.Negative = HighBitSet(value);
    }

    internal void Compare(byte a, byte b)
    {
        s.Carry = a >= b;
        s.Zero = a == b;
        s.Negative = HighBitSet((byte)(a - b));
    }

    internal byte AddWithCarry(byte value)
    {
        int carry = s.Carry ? 1 : 0;
        int sum = s.AC + value + carry;
        byte res = (byte)sum;
        s.Carry = sum > 0xFF;
        s.Overflow = HighBitSet((byte)(~(s.AC ^ value) & (s.AC ^ res)));
        return res;
    }

    internal byte SubWithBorrow(byte value) => AddWithCarry((byte)(value ^ 0xFF));

    internal byte ShiftLeft(byte value)
    {
        byte res = (byte)(value << 1);
        s.Carry = HighBitSet(value);
        SetZN(res);
        return res;
    }

    internal byte ShiftRight(byte value)
    {
        byte res = (byte)(value >> 1);
        s.Carry = LowBitSet(value);
        SetZN(res);
        return res;
    }

    internal byte RotateLeft(byte value)
    {
        int carry = s.Carry ? 1 : 0;
        byte res = (byte)((value << 1) | carry);
        s.Carry = HighBitSet(value);
        SetZN(res);
        return res;
    }

    internal byte RotateRight(byte value)
    {
        int carry = s.Carry ? 1 : 0;
        byte res = (byte)((value >> 1) | (carry << 7));
        s.Carry = LowBitSet(value);
        SetZN(res);
        return res;
    }

    private bool HighBitSet(byte value) => (value & 0b1000_0000) != 0;
    private bool LowBitSet(byte value) => (value & 0b0000_0001) != 0;
}
