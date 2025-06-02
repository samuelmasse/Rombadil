namespace Rombadil;

public class NesMemoryBus(
    CpuEmulatorMemory memory,
    CpuEmulatorState state,
    PpuNes ppu,
    NesController controller1,
    NesController controller2) : CpuEmulatorMemoryBus(memory)
{
    public override byte Read(ushort addr)
    {
        if (addr == 0x4016)
            return controller1.Read();
        else if (addr == 0x4017)
            return controller2.Read();
        else if (addr >= 0x2000 && addr <= 0x3FFF)
            return ppu.ReadRegister(addr);
        else return base.Read(addr);
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr == 0x4014)
        {
            ushort baseAddr = (ushort)(value << 8);
            byte start = ppu.OamAddr;
            for (int i = 0; i < 256; i++)
            {
                byte b = memory[(ushort)(baseAddr + i)];
                ppu.WriteOam((start + i) & 0xFF, b);
            }
        }
        else if (addr == 0x4016)
            controller1.Write(value);
        else if (addr == 0x4017)
            controller2.Write(value);
        else if (addr >= 0x2000 && addr <= 0x3FFF)
            ppu.WriteRegister(addr, value);
        else base.Write(addr, value);
    }
}
