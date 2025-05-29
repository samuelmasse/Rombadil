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
        s.SetFlag(CpuStatus.Zero, value == 0);
        s.SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
    }

    internal void Compare(byte a, byte b)
    {
        s.SetFlag(CpuStatus.Carry, a >= b);
        s.SetFlag(CpuStatus.Zero, a == b);
        s.SetFlag(CpuStatus.Negative, ((a - b) & 0x80) != 0);
    }

    internal byte AddWithCarry(byte value)
    {
        int a = s.AC;
        int m = value;
        int carryIn = s.SR.HasFlag(CpuStatus.Carry) ? 1 : 0;

        int sum = a + m + carryIn;
        byte result = (byte)sum;

        s.SetFlag(CpuStatus.Carry, sum > 0xFF);
        s.SetFlag(CpuStatus.Overflow, (~(a ^ m) & (a ^ result) & 0x80) != 0);

        return result;
    }

    internal byte SubWithBorrow(byte value) => AddWithCarry((byte)(value ^ 0xFF));

    internal byte ShiftLeft(byte value)
    {
        s.SetFlag(CpuStatus.Carry, (value & 0x80) != 0);
        byte result = (byte)(value << 1);
        SetZN(result);
        return result;
    }

    internal byte ShiftRight(byte value)
    {
        s.SetFlag(CpuStatus.Carry, (value & 0x01) != 0);
        byte result = (byte)(value >> 1);
        s.SetFlag(CpuStatus.Negative, false);
        s.SetFlag(CpuStatus.Zero, result == 0);
        return result;
    }

    internal byte RotateLeft(byte value)
    {
        int carryIn = s.HasFlag(CpuStatus.Carry) ? 1 : 0;
        bool carryOut = (value & 0x80) != 0;
        byte result = (byte)((value << 1) | carryIn);

        s.SetFlag(CpuStatus.Carry, carryOut);
        SetZN(result);
        return result;
    }

    internal byte RotateRight(byte value)
    {
        int carryIn = s.HasFlag(CpuStatus.Carry) ? 1 : 0;
        bool carryOut = (value & 0x01) != 0;
        byte result = (byte)((value >> 1) | (carryIn << 7));

        s.SetFlag(CpuStatus.Carry, carryOut);
        SetZN(result);
        return result;
    }
}
