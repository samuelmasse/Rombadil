using Rombadil.Cpu.Emulator;

namespace Rombadil;

public class NesMemoryBus(CpuEmulatorMemory memory, PpuNes ppu) : CpuEmulatorMemoryBus(memory)
{
    public override byte Read(ushort addr)
    {
        if (addr >= 0x2000 && addr <= 0x3FFF)
            return ppu.ReadRegister((ushort)(addr & 0x2007));
        else return base.Read(addr);
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x2000 && addr <= 0x3FFF)
            ppu.WriteRegister((ushort)(addr & 0x2007), value);
        else base.Write(addr, value);
    }
}
