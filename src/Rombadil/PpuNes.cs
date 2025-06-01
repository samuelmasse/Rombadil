namespace Rombadil;

public class PpuNes(Memory<byte> chrRom, Pixels pixels)
{
    private int cycles;
    private int cycle;
    private int scanline;

    private readonly byte[] vram = new byte[0x800];
    private readonly byte[] palette = new byte[32];

    private byte ctrl;
    private byte mask;
    private byte status;
    private byte buffer;
    private bool writeToggle;
    private ushort v;
    private ushort t;
    private byte x;
    private byte oamAddr;

    public byte Ctrl => ctrl;
    public ref int Cycles => ref cycles;

    public bool Step()
    {
        bool triggerNmi = false;

        if (scanline == 241 && cycle == 1)
        {
            status |= 0x80;
            triggerNmi = true;
        }

        if (scanline == 261 && cycle == 1)
        {
            status &= 0x7F;
            writeToggle = false;
        }

        if (scanline < 240 && cycle > 0 && cycle <= 256)
            RenderPixel(cycle - 1, scanline);

        cycle++;
        cycles++;

        if (cycle == 341)
        {
            cycle = 0;
            scanline = (scanline + 1) % 262;
        }

        return triggerNmi;
    }

    private void RenderPixel(int x, int y)
    {
        int tileX = x / 8;
        int tileY = y / 8;
        int tileIndex = vram[tileY * 32 + tileX];

        int fineY = y % 8;
        int bitplane1 = chrRom.Span[tileIndex * 16 + fineY];
        int bitplane2 = chrRom.Span[tileIndex * 16 + fineY + 8];

        int fineX = 7 - (x % 8);
        int bit0 = (bitplane1 >> fineX) & 1;
        int bit1 = (bitplane2 >> fineX) & 1;
        int paletteIndex = (bit1 << 1) | bit0;

        byte color = paletteIndex switch
        {
            0 => 0x01,
            1 => 0x22,
            2 => 0x44,
            3 => 0x66,
            _ => 0
        };

        pixels[(y * 256 + x) * 3 + 0] = color;
        pixels[(y * 256 + x) * 3 + 1] = color;
        pixels[(y * 256 + x) * 3 + 2] = color;
    }

    public byte ReadRegister(ushort reg) => (reg & 0x2007) switch
    {
        0x2002 => ReadStatus(),
        0x2004 => 0x00, // TODO: OAM read
        0x2007 => ReadData(),
        _ => 0x00
    };

    public void WriteRegister(ushort reg, byte value)
    {
        switch (reg & 0x2007)
        {
            case 0x2000: WriteCtrl(value); break;
            case 0x2001: mask = value; break;
            case 0x2003: oamAddr = value; break;
            case 0x2004: /* TODO: OAM write */ break;
            case 0x2005: WriteScroll(value); break;
            case 0x2006: WriteAddr(value); break;
            case 0x2007: WriteData(value); break;
        }
    }

    private byte ReadStatus()
    {
        byte result = (byte)(status & 0xE0);
        writeToggle = false;
        status &= 0x7F;
        return result;
    }

    private byte ReadData()
    {
        byte result = buffer;
        buffer = ReadPpuMemory(v);

        if (v >= 0x3F00)
            result = buffer;

        v += (ctrl & 0x04) != 0 ? (ushort)32 : (ushort)1;
        return result;
    }

    private void WriteCtrl(byte value)
    {
        ctrl = value;
        t = (ushort)((t & 0xF3FF) | ((value & 0x03) << 10));
    }

    private void WriteScroll(byte value)
    {
        if (!writeToggle)
        {
            t = (ushort)((t & 0xFFE0) | (value >> 3));
            x = (byte)(value & 0x07);
        }
        else
        {
            t = (ushort)((t & 0x8FFF) | ((value & 0x07) << 12));
            t = (ushort)((t & 0xFC1F) | ((value & 0xF8) << 2));
        }

        writeToggle = !writeToggle;
    }

    private void WriteAddr(byte value)
    {
        if (!writeToggle)
        {
            t = (ushort)((t & 0x00FF) | ((value & 0x3F) << 8));
        }
        else
        {
            t = (ushort)((t & 0xFF00) | value);
            v = t;
        }

        writeToggle = !writeToggle;
    }

    private void WriteData(byte value)
    {
        WritePpuMemory(v, value);
        v += (ctrl & 0x04) != 0 ? (ushort)32 : (ushort)1;
    }

    private byte ReadPpuMemory(ushort addr)
    {
        addr &= 0x3FFF;

        return addr switch
        {
            < 0x2000 => 0x00, // CHR ROM/RAM not yet implemented
            < 0x3F00 => vram[(addr - 0x2000) % 0x800],
            < 0x4000 => ReadPalette(addr),
            _ => 0
        };
    }

    private void WritePpuMemory(ushort addr, byte value)
    {
        addr &= 0x3FFF;

        if (addr < 0x2000)
        {
            // CHR ROM/RAM not yet implemented
        }
        else if (addr < 0x3F00)
        {
            vram[(addr - 0x2000) % 0x800] = value;
        }
        else if (addr < 0x4000)
        {
            WritePalette(addr, value);
        }
    }

    private byte ReadPalette(ushort addr)
    {
        int index = (addr - 0x3F00) % 32;
        if (index % 4 == 0) index &= 0x0F;
        return palette[index];
    }

    private void WritePalette(ushort addr, byte value)
    {
        int index = (addr - 0x3F00) % 32;
        if (index % 4 == 0) index &= 0x0F;
        palette[index] = value;
    }
}
