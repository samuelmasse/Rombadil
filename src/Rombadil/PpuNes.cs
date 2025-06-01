namespace Rombadil;

public class PpuNes(Memory<byte> chrRom, Pixels pixels)
{
    private static readonly (byte R, byte G, byte B)[] nespal =
    [
        (84, 84, 84), (0, 30, 116), (8, 16, 144), (48, 0, 136), (68, 0, 100), (92, 0, 48), (84, 4, 0), (60, 24, 0),
        (32, 42, 0), (8, 58, 0), (0, 64, 0), (0, 60, 0), (0, 50, 60), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (152, 150, 152), (8, 76, 196), (48, 50, 236), (92, 30, 228), (136, 20, 176), (160, 20, 100), (152, 34, 32), (120, 60, 0),
        (84, 90, 0), (40, 114, 0), (8, 124, 0), (0, 118, 40), (0, 102, 120), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (236, 238, 236), (76, 154, 236), (120, 124, 236), (176, 98, 236), (228, 84, 236), (236, 88, 180), (236, 106, 100), (212, 136, 32),
        (160, 170, 0), (116, 196, 0), (76, 208, 32), (56, 204, 108), (56, 180, 204), (60, 60, 60), (0, 0, 0), (0, 0, 0),
        (236, 238, 236), (168, 204, 236), (188, 188, 236), (212, 178, 236), (236, 174, 236), (236, 174, 212), (236, 180, 176), (228, 196, 144),
        (204, 210, 120), (180, 222, 120), (168, 226, 144), (152, 226, 180), (160, 214, 228), (160, 162, 160), (0, 0, 0), (0, 0, 0)
    ];

    private int cycles;
    private int cycle;
    private int scanline;
    private readonly bool[,] bgOpaque = new bool[240, 256];

    private readonly byte[] vram = new byte[0x800];
    private readonly byte[] palette = new byte[32];
    private readonly byte[] oam = new byte[256];

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

    public void Reset()
    {
        cycles = 0;
        cycle = 0;
        scanline = 0;
        writeToggle = false;
        v = 0;
        t = 0;
        x = 0;
        ctrl = 0;
        mask = 0;
        status = 0;
        buffer = 0;
        oamAddr = 0;
        Array.Clear(vram, 0, vram.Length);
        Array.Clear(palette, 0, palette.Length);
        Array.Clear(oam, 0, oam.Length);
    }

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
            status &= 0x1F;
            writeToggle = false;
        }

        if (scanline < 240 && cycle > 0 && cycle <= 256)
        {
            RenderPixel(cycle - 1, scanline);
            RenderSpritePixel(cycle - 1, scanline);
        }

        if (scanline < 240 && cycle == 257 && (mask & 0x18) != 0)
            v = (ushort)((v & 0xFBE0) | (t & 0x041F));

        cycle++;
        cycles++;

        if (cycle == 341)
        {
            cycle = 0;
            scanline = (scanline + 1) % 262;
        }

        return triggerNmi;
    }

    public byte ReadRegister(ushort reg)
    {
        return (reg % 8) switch
        {
            2 => ReadStatus(),
            4 => oam[oamAddr],
            7 => ReadData(),
            _ => 0x00,
        };
    }

    public void WriteRegister(ushort reg, byte value)
    {
        switch (reg % 8)
        {
            case 0: WriteCtrl(value); break;
            case 1: mask = value; break;
            case 3: oamAddr = value; break;
            case 4:
                oam[oamAddr] = value;
                oamAddr++;
                break;
            case 5: WriteScroll(value); break;
            case 6: WriteAddr(value); break;
            case 7: WriteData(value); break;
        }
    }

    public void WriteOam(int index, byte value) => oam[index] = value;

    private void RenderPixel(int xScreen, int yScreen)
    {
        int scrolledX = (xScreen + x) & 0x1FF;
        int coarseX = (v & 0x1F) + (scrolledX >> 3);
        int fineX = 7 - (scrolledX & 0x07);

        int tileX = coarseX & 0x1F;
        int nametableX = ((v >> 10) & 0x01) ^ ((coarseX >> 5) & 0x01);
        int nametableBase = 0x2000 + nametableX * 0x400;

        int tileY = yScreen / 8;
        int fineY = yScreen % 8;

        int tileIndex = ReadPpuMemory((ushort)(nametableBase + tileY * 32 + tileX));
        int patternTableBase = (ctrl & 0x10) != 0 ? 0x1000 : 0x0000;
        int tileAddr = patternTableBase + tileIndex * 16;

        byte plane0 = chrRom.Span[tileAddr + fineY];
        byte plane1 = chrRom.Span[tileAddr + fineY + 8];

        int bit0 = (plane0 >> fineX) & 1;
        int bit1 = (plane1 >> fineX) & 1;
        int colorIndex = (bit1 << 1) | bit0;
        bgOpaque[yScreen, xScreen] = colorIndex != 0;

        int paletteNum = 0;
        if (colorIndex != 0)
        {
            int attrX = tileX / 4;
            int attrY = tileY / 4;
            int attrIndex = attrY * 8 + attrX;
            byte attr = ReadPpuMemory((ushort)(nametableBase + 0x3C0 + attrIndex));

            int shift = ((tileY % 4) / 2) * 4 + ((tileX % 4) / 2) * 2;
            paletteNum = (attr >> shift) & 0x03;
        }

        ushort paletteAddr = (ushort)(0x3F00 + (paletteNum << 2) + colorIndex);
        var (r, g, b) = ReadPaletteColor(paletteAddr);

        int index = (yScreen * 256 + xScreen) * 3;
        pixels[index + 0] = r;
        pixels[index + 1] = g;
        pixels[index + 2] = b;
    }


    private void RenderSpritePixel(int x, int y)
    {
        if ((mask & 0x10) == 0)
            return;

        for (int i = 0; i < 64; i++)
        {
            int baseIndex = i * 4;
            int spriteY = oam[baseIndex + 0] + 1;
            byte tileIndex = oam[baseIndex + 1];
            byte attr = oam[baseIndex + 2];
            int spriteX = oam[baseIndex + 3];

            if (y < spriteY || y >= spriteY + 8)
                continue;

            int row = y - spriteY;
            if ((attr & 0x80) != 0) row = 7 - row;

            int patternTableBase = (ctrl & 0x08) != 0 ? 0x1000 : 0x0000;
            int addr = patternTableBase + tileIndex * 16 + row;
            byte plane0 = chrRom.Span[addr];
            byte plane1 = chrRom.Span[addr + 8];

            for (int col = 0; col < 8; col++)
            {
                int bit = (attr & 0x40) != 0 ? col : 7 - col;
                int bit0 = (plane0 >> bit) & 1;
                int bit1 = (plane1 >> bit) & 1;
                int colorIndex = (bit1 << 1) | bit0;

                if (colorIndex == 0)
                    continue;

                int px = spriteX + col;
                if (px != x)
                    continue;

                if (i == 0 && bgOpaque[y, x])
                    status |= 0x40;

                int paletteNum = attr & 0x03;
                ushort paletteAddr = (ushort)(0x3F10 + (paletteNum << 2) + colorIndex);
                var (r, g, b) = ReadPaletteColor(paletteAddr);

                int index = (y * 256 + x) * 3;
                pixels[index + 0] = r;
                pixels[index + 1] = g;
                pixels[index + 2] = b;
            }
        }
    }

    private (byte r, byte g, byte b) ReadPaletteColor(ushort addr)
    {
        addr &= 0x3FFF;
        if ((addr & 0x13) == 0x10)
            addr &= 0xFFEF;

        byte colorIndex = ReadPpuMemory(addr);
        return nespal[colorIndex];
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
        byte result;

        if (v < 0x3F00)
        {
            result = buffer;
            buffer = ReadPpuMemory(v);
        }
        else
        {
            result = ReadPpuMemory(v);
            buffer = ReadPpuMemory((ushort)(v - 0x1000));
        }

        v += (ctrl & 0x04) != 0 ? (ushort)32 : (ushort)1;
        return result;
    }

    private void WriteData(byte value)
    {
        WritePpuMemory(v, value);
        v += (ctrl & 0x04) != 0 ? (ushort)32 : (ushort)1;
    }

    private byte ReadPpuMemory(ushort addr)
    {
        addr &= 0x3FFF;

        if (addr < 0x2000)
            return chrRom.Span[addr];
        else if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;

            int index = addr - 0x2000;
            return vram[index % 0x800];
        }
        else if (addr < 0x4000)
            return ReadPalette(addr);

        return 0;
    }

    private void WritePpuMemory(ushort addr, byte value)
    {
        addr &= 0x3FFF;

        if (addr < 0x2000)
            chrRom.Span[addr] = value;
        else if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;

            int index = addr - 0x2000;
            vram[index % 0x800] = value;
        }
        else if (addr < 0x4000)
            WritePalette(addr, value);
    }

    private byte ReadPalette(ushort addr)
    {
        addr = (ushort)(0x3F00 + (addr % 32));

        if ((addr & 0x13) == 0x10)
            addr = (ushort)(addr & 0xFFEF);

        int index = addr - 0x3F00;
        return palette[index];
    }

    private void WritePalette(ushort addr, byte value)
    {
        addr = (ushort)(0x3F00 + (addr % 32));

        if ((addr & 0x13) == 0x10)
            addr = (ushort)(addr & 0xFFEF);

        int index = addr - 0x3F00;
        palette[index] = value;
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
            t = (ushort)((t & 0x00FF) | ((value & 0x3F) << 8));
        else
        {
            t = (ushort)((t & 0xFF00) | value);
            v = t;
        }

        writeToggle = !writeToggle;
    }
}
