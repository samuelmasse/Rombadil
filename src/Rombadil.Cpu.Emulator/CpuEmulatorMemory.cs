namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorMemory(Memory<byte> bytes, Memory<ushort> map)
{
    public ref byte this[ushort index] => ref bytes.Span[map.Span[index]];

    public ushort Word(ushort index) => (ushort)(this[index] | (this[(ushort)(index + 1)] << 8));
    public ushort WordZP(byte index) => (ushort)(this[index] | (this[(byte)(index + 1)] << 8));
    public ushort WordPageWrap(ushort index) => (ushort)(this[index] | (this[(ushort)((index & 0xFF00) | ((index + 1) & 0x00FF))] << 8));
}
