namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorBusMap(Memory<byte> memory, Memory<ushort> map) : CpuEmulatorBus
{
    public override byte Peek(ushort addr) => memory.Span[map.Span[addr]];
    public override byte Read(ushort addr) => memory.Span[map.Span[addr]];
    public override void Write(ushort addr, byte value) => memory.Span[map.Span[addr]] = value;
}
