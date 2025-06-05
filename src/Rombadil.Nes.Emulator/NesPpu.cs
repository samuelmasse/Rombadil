namespace Rombadil.Nes.Emulator;

public class NesPpu(NesMapper mapper, Memory<byte> framebuffer)
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
    private readonly byte[] prevSpriteHeight = new byte[64];

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
    private bool evenFrame = true;

    private int nmiDelay = 0;
    private bool nmiPending;
    private bool nmiSuppressed;
    private bool vblankSuppressed;

    public ref long Cycles => ref cycles;
    public bool PendingNmi => nmiPending;
    public byte OamAddr => oamAddr;

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
        bool renderingEnabled = (mask & 0x18) != 0;

        if (scanline == 241 && cycle == 1)
        {
            if (!vblankSuppressed)
            {
                status |= 0x80;
                if ((ctrl & 0x80) != 0)
                    nmiDelay = 9;
            }
        }

        if (nmiDelay > 0)
        {
            nmiDelay--;
            if (nmiDelay == 0 && !nmiSuppressed)
                nmiPending = true;
        }

        if (scanline == 261 && cycle == 1)
        {
            status &= 0x1F;
            writeToggle = false;
            nmiSuppressed = false;
            vblankSuppressed = false;
        }

        if (scanline < 240 && cycle > 0 && cycle <= 256)
        {
            RenderPixel(cycle - 1, scanline);
            RenderSpritePixel(cycle - 1, scanline);
            if (cycle == 256 && renderingEnabled)
                mapper.ClockIrq();
        }

        if ((scanline < 240 || scanline == 261) && cycle == 257 && renderingEnabled)
            v = (ushort)((v & 0xFBE0) | (t & 0x041F));

        if ((scanline < 240) && (cycle >= 328 || (cycle >= 1 && cycle <= 256)) && renderingEnabled)
        {
            if (cycle == 256)
                IncrementY();
        }

        if (scanline == 261 && cycle >= 280 && cycle <= 304 && renderingEnabled)
            v = (ushort)((v & 0x841F) | (t & 0x7BE0));

        cycle++;
        cycles++;

        if (cycle == 341)
        {
            cycle = 0;

            if (scanline == 261 && evenFrame && renderingEnabled)
                cycle = 1;

            scanline = (scanline + 1) % 262;

            if (scanline == 0)
                evenFrame = !evenFrame;
        }

        if (scanline == 261 && cycle == 340)
        {
            backBuffer.AsMemory().CopyTo(framebuffer);
            return true;
        }

        return false;
    }

    private void IncrementY()
    {
        if ((v & 0x7000) != 0x7000)
        {
            v += 0x1000;
        }
        else
        {
            v &= 0x8FFF;
            int y = (v & 0x03E0) >> 5;
            if (y == 29)
            {
                y = 0;
                v ^= 0x0800;
            }
            else if (y == 31)
            {
                y = 0;
            }
            else
            {
                y++;
            }
            v = (ushort)((v & 0xFC1F) | (y << 5));
        }
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

    public byte PeekRegister(ushort reg)
    {
        return (reg % 8) switch
        {
            2 => PeekStatus(),
            4 => oam[oamAddr],
            7 => PeekData(),
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

    public void ClearPendingNmi() => nmiPending = false;

    public void DmaCopy(Span<byte> source)
    {
        for (int i = 0; i < 256; i++)
            oam[(oamAddr + i) & 0xFF] = source[i];
    }

    private void AddSpriteToBlocks(int i)
    {
        int baseIndex = i * 4;
        int spriteY = oam[baseIndex + 0] + 1;
        int spriteX = oam[baseIndex + 3];
        int height = (ctrl & 0x20) != 0 ? 16 : 8;

        prevSpriteX[i] = (byte)spriteX;
        prevSpriteY[i] = (byte)spriteY;
        prevSpriteHeight[i] = (byte)height;

        int bx0 = spriteX / 16;
        int by0 = spriteY / 16;

        Add(0, 0);
        Add(1, 0);
        Add(0, 1);
        Add(1, 1);

        if (height > 8)
        {
            Add(0, 2);
            Add(1, 2);
        }

        void Add(int dx, int dy)
        {
            if (bx0 + dx < 16 && by0 + dy < 15)
                spriteBlocks[(byte)(by0 + dy), (byte)(bx0 + dx)][(byte)i] = 0;
        }
    }

    private void RemoveSpriteFromBlocks(int i)
    {
        int spriteX = prevSpriteX[i];
        int spriteY = prevSpriteY[i];
        int height = prevSpriteHeight[i];

        int bx0 = spriteX / 16;
        int by0 = spriteY / 16;

        Remove(0, 0);
        Remove(1, 0);
        Remove(0, 1);
        Remove(1, 1);

        if (height > 8)
        {
            Remove(0, 2);
            Remove(1, 2);
        }

        void Remove(int dx, int dy)
        {
            if (bx0 + dx < 16 && by0 + dy < 15)
                spriteBlocks[(byte)(by0 + dy), (byte)(bx0 + dx)].Remove((byte)i);
        }
    }

    private void RenderPixel(int xScreen, int yScreen)
    {
        bool showBg = (mask & 0x08) != 0;
        bool showBgLeft = (mask & 0x02) != 0;

        if (!showBg || (xScreen < 8 && !showBgLeft))
        {
            ushort pAddr = ((mask & 0x18) == 0) ? (ushort)(v & 0x3FFF) : (ushort)0x3F00;
            if (pAddr < 0x3F00 || pAddr >= 0x4000)
                pAddr = 0x3F00;

            var (r, g, b) = ReadPaletteColor(pAddr);
            var (er, eg, eb) = ApplyColorEmphasis(r, g, b);

            int bindex = (yScreen * 256 + xScreen) * 3;
            backBuffer[bindex + 0] = er;
            backBuffer[bindex + 1] = eg;
            backBuffer[bindex + 2] = eb;

            bgOpaque[yScreen, xScreen] = false;
            return;
        }

        int fineXScroll = x;
        int scrollX = (v & 0x001F);
        int scrollY = ((v >> 5) & 0x1F);
        int fineY = (v >> 12) & 0x7;

        int coarseX = (scrollX + ((xScreen + fineXScroll) / 8)) & 0x1F;
        int tileX = coarseX;
        int fineX = 7 - ((xScreen + fineXScroll) % 8);

        int nametableX = ((v >> 10) & 1);
        int nametableY = ((v >> 11) & 1);

        nametableX ^= ((scrollX + ((xScreen + fineXScroll) / 8)) >> 5) & 1;

        int nametableIndex = nametableY * 2 + nametableX;
        int nametableBase = 0x2000 + nametableIndex * 0x400;

        int tileY = scrollY;
        int tileIndex = ReadPpuMemory((ushort)(nametableBase + tileY * 32 + tileX));

        int patternTableBase = (ctrl & 0x10) != 0 ? 0x1000 : 0x0000;
        int tileAddr = patternTableBase + tileIndex * 16;

        byte plane0 = mapper.ReadChr((ushort)(tileAddr + fineY));
        byte plane1 = mapper.ReadChr((ushort)(tileAddr + fineY + 8));

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
        var (baseR, baseG, baseB) = ReadPaletteColor(paletteAddr);
        var (emphR, emphG, emphB) = ApplyColorEmphasis(baseR, baseG, baseB);

        int index = (yScreen * 256 + xScreen) * 3;
        backBuffer[index + 0] = emphR;
        backBuffer[index + 1] = emphG;
        backBuffer[index + 2] = emphB;
    }

    private (byte r, byte g, byte b) ApplyColorEmphasis(byte r, byte g, byte b)
    {
        if (r == 0 && g == 0 && b == 0)
            return (r, g, b);

        const double attenuation = 0.816328;

        bool emphasizeRed = (mask & 0x20) != 0;
        bool emphasizeGreen = (mask & 0x40) != 0;
        bool emphasizeBlue = (mask & 0x80) != 0;

        double rf = r;
        double gf = g;
        double bf = b;

        if (emphasizeRed) { gf *= attenuation; bf *= attenuation; }
        if (emphasizeGreen) { rf *= attenuation; bf *= attenuation; }
        if (emphasizeBlue) { rf *= attenuation; gf *= attenuation; }

        return ((byte)Math.Clamp(rf, 0, 255),
                (byte)Math.Clamp(gf, 0, 255),
                (byte)Math.Clamp(bf, 0, 255));
    }

    private void RenderSpritePixel(int x, int y)
    {
        if ((mask & 0x10) == 0)
            return;

        bool spriteSize8x16 = (ctrl & 0x20) != 0;

        int blockX = x / 16;
        int blockY = y / 16;
        var list = spriteBlocks[blockY, blockX].Keys;

        for (int i = 0; i < list.Count; i++)
        {
            int spriteIndex = list[i];
            int baseIndex = spriteIndex * 4;
            int spriteX = oam[baseIndex + 3];
            int col = x - spriteX;
            if (col >= 8 || col < 0)
                continue;

            int spriteY = oam[baseIndex + 0] + 1;
            int height = spriteSize8x16 ? 16 : 8;
            if (y < spriteY || y >= spriteY + height)
                continue;

            byte tileIndex = oam[baseIndex + 1];
            byte attr = oam[baseIndex + 2];

            int row = y - spriteY;
            if ((attr & 0x80) != 0)
                row = height - 1 - row;

            ushort addr;
            if (spriteSize8x16)
            {
                byte tile = (byte)(tileIndex & 0xFE);
                int table = (tileIndex & 1) != 0 ? 0x1000 : 0x0000;
                addr = (ushort)(table + tile * 16 + (row % 8));
                if (row >= 8) addr += 16;
            }
            else
            {
                int patternTableBase = (ctrl & 0x08) != 0 ? 0x1000 : 0x0000;
                addr = (ushort)(patternTableBase + tileIndex * 16 + row);
            }

            byte plane0 = mapper.ReadChr(addr);
            byte plane1 = mapper.ReadChr((ushort)(addr + 8));

            int bit = (attr & 0x40) != 0 ? col : 7 - col;
            int bit0 = (plane0 >> bit) & 1;
            int bit1 = (plane1 >> bit) & 1;
            int colorIndex = (bit1 << 1) | bit0;

            if (colorIndex == 0)
                continue;

            if (spriteIndex == 0 && bgOpaque[y, x])
                status |= 0x40;

            bool behindBg = (attr & 0x20) != 0;
            if (behindBg && bgOpaque[y, x])
                return;

            int paletteNum = attr & 0x03;
            ushort paletteAddr = (ushort)(0x3F10 + (paletteNum << 2) + colorIndex);
            var (r, g, b) = ReadPaletteColor(paletteAddr);

            int index = (y * 256 + x) * 3;
            backBuffer[index + 0] = r;
            backBuffer[index + 1] = g;
            backBuffer[index + 2] = b;

            return;
        }
    }


    private (byte r, byte g, byte b) ReadPaletteColor(ushort addr)
    {
        addr &= 0x3FFF;
        if ((addr & 0x13) == 0x10)
            addr &= 0xFFEF;

        byte colorIndex = ReadPpuMemory(addr);
        colorIndex &= 0x3F;

        return nespal[colorIndex];
    }

    private byte ReadStatus()
    {
        if (scanline == 241 && cycle <= 3 && cycle > 0)
        {
            nmiSuppressed = true;
            vblankSuppressed = true;
        }
        byte result = PeekStatus();
        writeToggle = false;
        status &= 0x7F;
        return result;
    }

    private byte PeekStatus() => (byte)(status & 0xE0);

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

    private byte PeekData()
    {
        if (v < 0x3F00)
            return buffer;
        else return ReadPpuMemory(v);
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
            return mapper.ReadChr(addr);
        else if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;

            int index = mapper.MapNametableAddr(addr);
            return vram[index];
        }
        else if (addr < 0x4000)
            return ReadPalette(addr);

        return 0;
    }

    private void WritePpuMemory(ushort addr, byte value)
    {
        addr &= 0x3FFF;

        if (addr < 0x2000)
            mapper.WriteChr(addr, value);
        else if (addr < 0x3F00)
        {
            if (addr >= 0x3000)
                addr -= 0x1000;

            int index = mapper.MapNametableAddr(addr);
            vram[index] = value;
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

        if (!prevNmiEnable && (value & 0x80) != 0 && (status & 0x80) != 0)
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
