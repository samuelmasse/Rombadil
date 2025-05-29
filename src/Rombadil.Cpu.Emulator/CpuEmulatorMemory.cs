namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorMemory(Memory<byte> bytes, Memory<ushort> map)
{
    public ref byte this[ushort index] => ref bytes.Span[map.Span[index]];
}
