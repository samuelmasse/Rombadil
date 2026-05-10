namespace Rombadil.Nes.Emulator;

public class NesPpuMemory(NesMapper mapper)
{
    private readonly byte[] vram = new byte[0x1000];
    private readonly byte[] palette = new byte[32];

    public void Reset()
    {
        Array.Clear(vram);
        Array.Clear(palette);
    }

    public byte Read(ushort addr)
    {
        addr &= 0x3FFF;
        if (addr < 0x2000)
            return mapper.ReadChr(addr);

        if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;
            return vram[mapper.MapNametableAddr(addr)];
        }

        if (addr < 0x4000)
            return ReadPalette(addr);

        return 0;
    }

    public void Write(ushort addr, byte value)
    {
        addr &= 0x3FFF;
        if (addr < 0x2000)
        {
            mapper.WriteChr(addr, value);
            return;
        }

        if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;
            vram[mapper.MapNametableAddr(addr)] = value;
            return;
        }

        if (addr < 0x4000)
            WritePalette(addr, value);
    }

    public byte ReadPalette(ushort addr)
    {
        addr = (ushort)(0x3F00 + (addr % 32));
        if ((addr & 0x13) == 0x10)
            addr = (ushort)(addr & 0xFFEF);
        return palette[addr - 0x3F00];
    }

    public void WritePalette(ushort addr, byte value)
    {
        addr = (ushort)(0x3F00 + (addr % 32));
        if ((addr & 0x13) == 0x10)
            addr = (ushort)(addr & 0xFFEF);
        palette[addr - 0x3F00] = value;
    }
}
