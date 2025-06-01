namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorMemoryBus(CpuEmulatorMemory memory)
{
    public virtual byte Read(ushort addr) => memory[addr];
    public virtual void Write(ushort addr, byte value) => memory[addr] = value;
}
