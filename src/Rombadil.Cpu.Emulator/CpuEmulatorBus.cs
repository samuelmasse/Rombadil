namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorBus
{
    private readonly byte[] memory = new byte[0x10000];

    public virtual byte Peek(ushort addr) => memory[addr];
    public virtual byte Read(ushort addr) => memory[addr];
    public virtual void Write(ushort addr, byte value) => memory[addr] = value;

    public byte this[ushort index]
    {
        get => Read(index);
        set => Write(index, value);
    }

    public ushort Word(ushort index) => (ushort)(this[index] | (this[(ushort)(index + 1)] << 8));
    public ushort WordZP(byte index) => (ushort)(this[index] | (this[(byte)(index + 1)] << 8));
    public ushort WordPageWrap(ushort index) => (ushort)(this[index] | (this[(ushort)((index & 0xFF00) | ((index + 1) & 0x00FF))] << 8));
}
