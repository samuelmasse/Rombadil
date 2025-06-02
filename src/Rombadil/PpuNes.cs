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

    private readonly byte[] backBuffer = new byte[256 * 240 * 3];
    private readonly SortedList<byte, byte>[,] spriteBlocks = new SortedList<byte, byte>[15, 16];
    private readonly byte[] prevSpriteX = new byte[64];
    private readonly byte[] prevSpriteY = new byte[64];

    private long cycles;
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
    private int scheduledNmi = -1;

    public byte Ctrl => ctrl;
    public ref long Cycles => ref cycles;
    public ref int ScheduledNmi => ref scheduledNmi;

    private int nmiDelay = 0;
    private bool nmiPending;
    private bool nmiSuppressed;

    public bool PendingNmi => nmiPending;

    public void ClearPendingNmi()
    {
        nmiPending = false;
    }

    public void Reset()
    {
        for (int y = 0; y < 15; y++)
            for (int x = 0; x < 16; x++)
                spriteBlocks[y, x] = new(32);

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
        Array.Clear(vram);
        Array.Clear(palette);
        Array.Clear(oam);
        Array.Clear(backBuffer);
    }

    public bool Step()
    {
        if (scanline == 241 && cycle == 1)
        {
            status |= 0x80;
            if ((ctrl & 0x80) != 0)
                nmiDelay = 9;
        }

        if (nmiDelay > 0)
        {
            nmiDelay--;
            if (nmiDelay == 0)
            {
                if (!nmiSuppressed)
                    nmiPending = true;
            }
        }

        if (scanline == 261 && cycle == 1)
        {
            status &= 0x1F;
            writeToggle = false;
            nmiSuppressed = false;
        }

        if (scanline < 240 && cycle > 0 && cycle <= 256)
        {
            RenderPixel(cycle - 1, scanline);
            RenderSpritePixel(cycle - 1, scanline);
        }

        if ((scanline < 240 || scanline == 261) && cycle == 257 && (mask & 0x18) != 0)
            v = (ushort)((v & 0xFBE0) | (t & 0x041F));

        cycle++;
        cycles++;

        if (cycle == 341)
        {
            cycle = 0;
            scanline = (scanline + 1) % 262;
        }

        if (scanline == 261 && cycle == 340)
        {
            Array.Copy(backBuffer, pixels.Data, pixels.Data.Length);
            return true;
        }
        else return false;
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
                WriteOam(oamAddr, value);
                oamAddr++;
                break;
            case 5: WriteScroll(value); break;
            case 6: WriteAddr(value); break;
            case 7: WriteData(value); break;
        }
    }

    public void WriteOam(int index, byte value)
    {
        int field = index % 4;
        int sprite = index / 4;

        if (field == 0 || field == 3)
            RemoveSpriteFromBlocks(sprite);

        oam[index] = value;

        if (field == 0 || field == 3)
            AddSpriteToBlocks(sprite);
    }

    private void AddSpriteToBlocks(int i)
    {
        int baseIndex = i * 4;
        int spriteY = oam[baseIndex + 0] + 1;
        int spriteX = oam[baseIndex + 3];

        prevSpriteX[i] = (byte)spriteX;
        prevSpriteY[i] = (byte)spriteY;

        int bx0 = spriteX / 16;
        int by0 = spriteY / 16;
        int bx1 = (spriteX + 7) / 16;
        int by1 = (spriteY + 7) / 16;

        if (bx0 < 16 && by0 < 15) spriteBlocks[by0, bx0].Add((byte)i, 0); // top-left
        if (bx1 < 16 && by0 < 15 && bx1 != bx0) spriteBlocks[by0, bx1].Add((byte)i, 0); // top-right
        if (bx0 < 16 && by1 < 15 && by1 != by0) spriteBlocks[by1, bx0].Add((byte)i, 0); // bottom-left
        if (bx1 < 16 && by1 < 15 && bx1 != bx0 && by1 != by0) spriteBlocks[by1, bx1].Add((byte)i, 0); // bottom-right
    }

    private void RemoveSpriteFromBlocks(int i)
    {
        int spriteX = prevSpriteX[i];
        int spriteY = prevSpriteY[i];

        int bx0 = spriteX / 16;
        int by0 = spriteY / 16;
        int bx1 = (spriteX + 7) / 16;
        int by1 = (spriteY + 7) / 16;

        if (bx0 < 16 && by0 < 15) spriteBlocks[by0, bx0].Remove((byte)i);
        if (bx1 < 16 && by0 < 15 && bx1 != bx0) spriteBlocks[by0, bx1].Remove((byte)i);
        if (bx0 < 16 && by1 < 15 && by1 != by0) spriteBlocks[by1, bx0].Remove((byte)i);
        if (bx1 < 16 && by1 < 15 && bx1 != bx0 && by1 != by0) spriteBlocks[by1, bx1].Remove((byte)i);
    }

    private void RenderPixel(int xScreen, int yScreen)
    {
        if ((mask & 0x08) == 0)
        {
            bgOpaque[yScreen, xScreen] = false;
            return;
        }

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
        backBuffer[index + 0] = r;
        backBuffer[index + 1] = g;
        backBuffer[index + 2] = b;
    }


    private void RenderSpritePixel(int x, int y)
    {
        if ((mask & 0x10) == 0)
            return;

        int blockX = x / 16;
        int blockY = y / 16;
        var list = spriteBlocks[blockY, blockX].Keys;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            int spriteIndex = list[i];
            int baseIndex = spriteIndex * 4;
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

                if (spriteIndex == 0 && bgOpaque[y, x])
                    status |= 0x40;

                bool behindBg = (attr & 0x20) != 0;
                if (behindBg && bgOpaque[y, x])
                    continue;

                int paletteNum = attr & 0x03;
                ushort paletteAddr = (ushort)(0x3F10 + (paletteNum << 2) + colorIndex);
                var (r, g, b) = ReadPaletteColor(paletteAddr);

                int index = (y * 256 + x) * 3;
                backBuffer[index + 0] = r;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = b;
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
        if (scanline == 241 && cycle <= 3 && cycle > 0)
            nmiSuppressed = true;
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
        bool prevNmiEnable = (ctrl & 0x80) != 0;
        ctrl = value;
        t = (ushort)((t & 0xF3FF) | ((value & 0x03) << 10));

        if (!prevNmiEnable && (value & 0x80) != 0 && (scanline > 240 && !(scanline == 261 && cycle >= 1)))
            nmiDelay = 9;
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
