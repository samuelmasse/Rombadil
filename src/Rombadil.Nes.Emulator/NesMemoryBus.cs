namespace Rombadil.Nes.Emulator;

public class NesMemoryBus(
    CpuEmulatorState state,
    NesMapper mapper,
    NesPpu ppu,
    NesApu apu,
    NesController controller1,
    NesController controller2) : CpuEmulatorBus
{
    public override byte Peek(ushort addr)
    {
        if (addr == 0x4015)
            return apu.PeekStatus();
        else if (addr == 0x4016)
            return controller1.Peek();
        else if (addr == 0x4017)
            return controller2.Peek();
        else if (addr >= 0x2000 && addr <= 0x3FFF)
            return ppu.PeekRegister(addr);
        else if (addr >= 0x8000)
            return mapper.Read(addr);
        else return base.Peek(addr);
    }

    public override byte Read(ushort addr)
    {
        if (addr == 0x4015)
        {
            while (apu.Cycles < state.Cycles)
                apu.Step();
            return apu.ReadStatus();
        }
        else if (addr == 0x4016)
            return controller1.Read();
        else if (addr == 0x4017)
            return controller2.Read();
        else if (addr >= 0x2000 && addr <= 0x3FFF)
            return ppu.ReadRegister(addr);
        else if (addr >= 0x8000)
            return mapper.Read(addr);
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
                byte b = Read((ushort)(baseAddr + i));
                ppu.WriteOam((start + i) & 0xFF, b);
            }
        }
        else if (addr == 0x4016)
        {
            controller1.Write(value);
            controller2.Write(value);
        }
        else if (addr >= 0x4000 && addr <= 0x4017 && addr != 0x4016)
        {
            while (apu.Cycles < state.Cycles)
                apu.Step();
            apu.WriteRegister(addr, value);
        }
        else if (addr >= 0x2000 && addr <= 0x3FFF)
            ppu.WriteRegister(addr, value);
        else if (addr >= 0x8000)
            mapper.Write(addr, value);
        else base.Write(addr, value);
    }
}
