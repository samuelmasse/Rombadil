namespace Rombadil.Nes.Emulator;

public class NesPpuSprite(NesMapper mapper)
{
    private const byte CtrlSpritePatternTable = 0x08;
    private const byte CtrlSprite8x16 = 0x20;
    private const byte SpriteAttrPaletteMask = 0x03;
    private const byte SpriteAttrBehindBg = 0x20;
    private const byte SpriteAttrFlipHorizontal = 0x40;
    private const byte SpriteAttrFlipVertical = 0x80;
    private const int MaxSpritesPerLine = 8;

    private readonly byte[] oam = new byte[256];
    private readonly byte[] secondaryOam = new byte[MaxSpritesPerLine * 4];
    private readonly byte[] spriteShiftLo = new byte[MaxSpritesPerLine];
    private readonly byte[] spriteShiftHi = new byte[MaxSpritesPerLine];
    private readonly byte[] spriteAttr = new byte[MaxSpritesPerLine];
    private readonly byte[] spriteX = new byte[MaxSpritesPerLine];

    private int spriteCountForNextLine;
    private bool spriteZeroOnNextLine;
    private int spriteCount;
    private bool spriteZeroOnLine;

    public bool SpriteZeroOnLine => spriteZeroOnLine;

    public byte ReadOam(byte addr) => oam[addr];
    public void WriteOam(int addr, byte value) => oam[addr] = value;

    public void Reset()
    {
        spriteCountForNextLine = 0;
        spriteCount = 0;
        spriteZeroOnNextLine = false;
        spriteZeroOnLine = false;
        Array.Clear(oam);
        Array.Clear(secondaryOam);
        Array.Clear(spriteShiftLo);
        Array.Clear(spriteShiftHi);
        Array.Clear(spriteAttr);
        Array.Clear(spriteX);
    }

    public void ClearSecondaryOam()
    {
        for (int i = 0; i < secondaryOam.Length; i++)
            secondaryOam[i] = 0xFF;
    }

    public bool EvaluateForNextLine(int targetLine, byte ctrl)
    {
        int spriteHeight = (ctrl & CtrlSprite8x16) != 0 ? 16 : 8;
        int found = 0;
        spriteZeroOnNextLine = false;
        bool overflow = false;

        for (int n = 0; n < 64; n++)
        {
            int spriteY = oam[n * 4];
            int row = targetLine - (spriteY + 1);
            if (row < 0 || row >= spriteHeight)
                continue;

            if (found >= MaxSpritesPerLine)
            {
                overflow = true;
                break;
            }

            secondaryOam[found * 4 + 0] = oam[n * 4 + 0];
            secondaryOam[found * 4 + 1] = oam[n * 4 + 1];
            secondaryOam[found * 4 + 2] = oam[n * 4 + 2];
            secondaryOam[found * 4 + 3] = oam[n * 4 + 3];
            if (n == 0)
                spriteZeroOnNextLine = true;
            found++;
        }

        spriteCountForNextLine = found;
        return overflow;
    }

    public void LoadShifters(int targetLine, byte ctrl)
    {
        int spriteHeight = (ctrl & CtrlSprite8x16) != 0 ? 16 : 8;

        for (int slot = 0; slot < MaxSpritesPerLine; slot++)
        {
            if (slot >= spriteCountForNextLine)
            {
                spriteShiftLo[slot] = 0;
                spriteShiftHi[slot] = 0;
                spriteAttr[slot] = 0;
                spriteX[slot] = 0xFF;
                continue;
            }

            int spriteY = secondaryOam[slot * 4 + 0];
            byte tileIndex = secondaryOam[slot * 4 + 1];
            byte attr = secondaryOam[slot * 4 + 2];
            byte xPos = secondaryOam[slot * 4 + 3];
            int row = targetLine - (spriteY + 1);
            if ((attr & SpriteAttrFlipVertical) != 0)
                row = spriteHeight - 1 - row;

            ushort patternAddr = ComputePatternAddress(tileIndex, row, spriteHeight, ctrl);
            byte patternLow = mapper.ReadChr(patternAddr);
            byte patternHigh = mapper.ReadChr((ushort)(patternAddr + 8));

            if ((attr & SpriteAttrFlipHorizontal) != 0)
            {
                patternLow = ReverseBits(patternLow);
                patternHigh = ReverseBits(patternHigh);
            }

            spriteShiftLo[slot] = patternLow;
            spriteShiftHi[slot] = patternHigh;
            spriteAttr[slot] = attr;
            spriteX[slot] = xPos;
        }

        spriteCount = spriteCountForNextLine;
        spriteZeroOnLine = spriteZeroOnNextLine;
    }

    public void TickShifters()
    {
        for (int i = 0; i < spriteCount; i++)
        {
            if (spriteX[i] > 0)
                spriteX[i]--;
            else
            {
                spriteShiftLo[i] <<= 1;
                spriteShiftHi[i] <<= 1;
            }
        }
    }

    public (int pixel, int palette, bool behindBg, int slot) SamplePixel()
    {
        for (int i = 0; i < spriteCount; i++)
        {
            if (spriteX[i] != 0)
                continue;

            int b0 = (spriteShiftLo[i] >> 7) & 1;
            int b1 = (spriteShiftHi[i] >> 7) & 1;
            int pixel = (b1 << 1) | b0;
            if (pixel == 0)
                continue;

            int palette = spriteAttr[i] & SpriteAttrPaletteMask;
            bool behindBg = (spriteAttr[i] & SpriteAttrBehindBg) != 0;
            return (pixel, palette, behindBg, i);
        }

        return (0, 0, false, -1);
    }

    private static ushort ComputePatternAddress(byte tileIndex, int row, int spriteHeight, byte ctrl)
    {
        if (spriteHeight == 16)
        {
            int patternTable = (tileIndex & 1) != 0 ? 0x1000 : 0;
            byte topTile = (byte)(tileIndex & 0xFE);
            int bottomOffset = row >= 8 ? 16 : 0;
            return (ushort)(patternTable + topTile * 16 + bottomOffset + (row & 7));
        }

        int tablePtr = (ctrl & CtrlSpritePatternTable) != 0 ? 0x1000 : 0;
        return (ushort)(tablePtr + tileIndex * 16 + row);
    }

    private static byte ReverseBits(byte value)
    {
        value = (byte)(((value & 0xF0) >> 4) | ((value & 0x0F) << 4));
        value = (byte)(((value & 0xCC) >> 2) | ((value & 0x33) << 2));
        value = (byte)(((value & 0xAA) >> 1) | ((value & 0x55) << 1));
        return value;
    }
}
