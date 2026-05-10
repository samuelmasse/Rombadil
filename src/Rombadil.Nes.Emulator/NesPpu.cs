namespace Rombadil.Nes.Emulator;

public class NesPpu
{
    public const int ScreenWidth = 256;
    public const int ScreenHeight = 240;
    private const int ScanlinesPerFrame = 262;
    private const int PreRenderScanline = 261;
    private const int VBlankStartScanline = 241;
    private const int CyclesPerScanline = 341;
    private const int NmiDelayCycles = 9;
    private const byte CtrlNmiEnable = 0x80;
    private const byte MaskGrayscale = 0x01;
    private const byte MaskShowBgLeft = 0x02;
    private const byte MaskShowSpriteLeft = 0x04;
    private const byte MaskShowBg = 0x08;
    private const byte MaskShowSprites = 0x10;
    private const byte MaskRenderingEnabled = MaskShowBg | MaskShowSprites;
    private const byte MaskEmphasisShift = 5;
    private const byte StatusSpriteOverflow = 0x20;
    private const byte StatusSpriteZeroHit = 0x40;
    private const byte StatusVBlank = 0x80;

    private readonly NesMapper mapper;
    private readonly Memory<byte> framebuffer;
    private readonly NesPpuMemory memory;
    private readonly NesPpuBackground bg;
    private readonly NesPpuSprite sprites;
    private readonly byte[] backBuffer;

    private long cycles;
    private int cycle;
    private int scanline;
    private bool evenFrame;
    private byte ctrl;
    private byte mask;
    private byte status;
    private byte oamAddr;
    private int nmiDelay;
    private bool nmiPending;
    private bool nmiSuppressed;
    private bool vblankSuppressed;
    private bool renderingPreviousCycle;

    public long Cycles => cycles;
    public bool PendingNmi => nmiPending;
    public byte OamAddr => oamAddr;
    private bool NmiOutput => (ctrl & CtrlNmiEnable) != 0 && (status & StatusVBlank) != 0;
    private bool InVblankNmiSuppressionWindow => scanline == VBlankStartScanline && cycle > 0 && cycle <= 3;
    private bool InVblankClearNmiEnableRace => scanline == PreRenderScanline && cycle == 1;

    public NesPpu(NesMapper mapper, Memory<byte> framebuffer)
    {
        this.mapper = mapper;
        this.framebuffer = framebuffer;
        memory = new NesPpuMemory(mapper);
        bg = new NesPpuBackground(memory, mapper);
        sprites = new NesPpuSprite(mapper);
        backBuffer = new byte[ScreenWidth * ScreenHeight * 3];
    }

    public void Reset()
    {
        cycles = 0;
        cycle = 0;
        scanline = 0;
        evenFrame = true;
        ctrl = 0;
        mask = 0;
        status = 0;
        oamAddr = 0;
        nmiDelay = 0;
        nmiPending = false;
        nmiSuppressed = false;
        vblankSuppressed = false;
        renderingPreviousCycle = false;
        memory.Reset();
        bg.Reset();
        sprites.Reset();
        Array.Clear(backBuffer);
    }

    public bool Step()
    {
        bool rendering = (mask & MaskRenderingEnabled) != 0;
        bool visibleLine = scanline < ScreenHeight;
        bool prerenderLine = scanline == PreRenderScanline;
        bool fetchLine = visibleLine || prerenderLine;

        UpdateVBlankAndNmi(prerenderLine);

        if (visibleLine && cycle >= 1 && cycle <= ScreenWidth)
            OutputPixel(cycle - 1, scanline);

        if (fetchLine && rendering)
            bg.RunPipeline(cycle, prerenderLine, ctrl);

        RunSpritePipeline(visibleLine, prerenderLine);

        if (visibleLine && cycle >= 1 && cycle <= ScreenWidth)
            sprites.TickShifters();

        if (fetchLine && cycle == 260 && rendering)
            mapper.ClockIrq();

        bool frameCompleted = AdvanceCycleAndScanline(prerenderLine, renderingPreviousCycle);
        renderingPreviousCycle = rendering;

        if (frameCompleted || scanline == PreRenderScanline && cycle == CyclesPerScanline - 1)
        {
            backBuffer.AsMemory().CopyTo(framebuffer);
            return true;
        }

        return false;
    }

    private void UpdateVBlankAndNmi(bool prerenderLine)
    {
        if (scanline == VBlankStartScanline && cycle == 1 && !vblankSuppressed)
        {
            status |= StatusVBlank;
            if ((ctrl & CtrlNmiEnable) != 0)
                nmiDelay = NmiDelayCycles;
        }

        if (nmiDelay > 0)
        {
            nmiDelay--;
            if (nmiDelay == 0 && !nmiSuppressed)
                nmiPending = true;
        }

        if (prerenderLine && cycle == 1)
        {
            status &= 0x1F;
            nmiSuppressed = false;
            vblankSuppressed = false;
        }
    }

    private void RunSpritePipeline(bool visibleLine, bool prerenderLine)
    {
        if (visibleLine)
        {
            if (cycle == 1)
                sprites.ClearSecondaryOam();
            else if (cycle == 257)
                EvaluateAndLoadSprites(scanline + 1);
        }

        if (prerenderLine && cycle == 257)
        {
            sprites.ClearSecondaryOam();
            EvaluateAndLoadSprites(0);
        }
    }

    private void EvaluateAndLoadSprites(int targetLine)
    {
        if (sprites.EvaluateForNextLine(targetLine, ctrl))
            status |= StatusSpriteOverflow;
        sprites.LoadShifters(targetLine, ctrl);
    }

    private bool AdvanceCycleAndScanline(bool prerenderLine, bool rendering)
    {
        if (prerenderLine && !evenFrame && rendering && cycle == CyclesPerScanline - 2)
        {
            cycles++;
            SkipOddFrameCycle();
            return true;
        }

        cycle++;
        cycles++;

        if (cycle != CyclesPerScanline)
            return false;

        cycle = 0;

        scanline = (scanline + 1) % ScanlinesPerFrame;
        if (scanline == 0)
            evenFrame = !evenFrame;

        return false;
    }

    private void OutputPixel(int xScreen, int yScreen)
    {
        bool showBg = (mask & MaskShowBg) != 0;
        bool showSprites = (mask & MaskShowSprites) != 0;
        bool showBgLeft = (mask & MaskShowBgLeft) != 0;
        bool showSpriteLeft = (mask & MaskShowSpriteLeft) != 0;
        bool grayscale = (mask & MaskGrayscale) != 0;

        int bgPixel = 0;
        int bgPalette = 0;
        if (showBg && (xScreen >= 8 || showBgLeft))
            (bgPixel, bgPalette) = bg.SamplePixel();

        int spritePixel = 0;
        int spritePalette = 0;
        bool spriteBehindBg = false;
        int spriteSlot = -1;
        if (showSprites && (xScreen >= 8 || showSpriteLeft))
            (spritePixel, spritePalette, spriteBehindBg, spriteSlot) = sprites.SamplePixel();

        DetectSpriteZeroHit(xScreen, bgPixel, spritePixel, spriteSlot, showBgLeft, showSpriteLeft);

        ushort paletteAddr = showBg || showSprites
            ? SelectPaletteAddress(bgPixel, bgPalette, spritePixel, spritePalette, spriteBehindBg)
            : SelectForcedBlankingPaletteAddress();
        byte colorIndex = memory.ReadPalette(paletteAddr);
        if (grayscale)
            colorIndex &= 0x30;

        int emphasis = (mask >> MaskEmphasisShift) & 7;
        int rgbIndex = (emphasis * 64 + (colorIndex & 0x3F)) * 3;
        int dst = (yScreen * ScreenWidth + xScreen) * 3;
        backBuffer[dst + 0] = NesPpuPalette.Rgb[rgbIndex + 0];
        backBuffer[dst + 1] = NesPpuPalette.Rgb[rgbIndex + 1];
        backBuffer[dst + 2] = NesPpuPalette.Rgb[rgbIndex + 2];
    }

    private ushort SelectForcedBlankingPaletteAddress()
    {
        ushort addr = bg.Address;
        if ((addr & 0x3F00) == 0x3F00)
            return addr;
        return 0x3F00;
    }

    private void DetectSpriteZeroHit(int xScreen, int bgPixel, int spritePixel, int spriteSlot, bool showBgLeft, bool showSpriteLeft)
    {
        if (spriteSlot != 0 || !sprites.SpriteZeroOnLine)
            return;
        if (bgPixel == 0 || spritePixel == 0)
            return;
        if (xScreen >= 255)
            return;
        if (xScreen < 8 && !(showBgLeft && showSpriteLeft))
            return;

        status |= StatusSpriteZeroHit;
    }

    private static ushort SelectPaletteAddress(int bgPixel, int bgPalette, int spritePixel, int spritePalette, bool spriteBehindBg)
    {
        if (bgPixel == 0 && spritePixel == 0)
            return 0x3F00;
        if (bgPixel == 0)
            return (ushort)(0x3F10 + (spritePalette << 2) + spritePixel);
        if (spritePixel == 0)
            return (ushort)(0x3F00 + (bgPalette << 2) + bgPixel);
        if (spriteBehindBg)
            return (ushort)(0x3F00 + (bgPalette << 2) + bgPixel);
        return (ushort)(0x3F10 + (spritePalette << 2) + spritePixel);
    }

    public byte ReadRegister(ushort reg)
    {
        return (reg % 8) switch
        {
            2 => ReadStatus(),
            4 => sprites.ReadOam(oamAddr),
            7 => bg.ReadData(ctrl),
            _ => 0x00,
        };
    }

    public byte PeekRegister(ushort reg)
    {
        return (reg % 8) switch
        {
            2 => PeekStatus(),
            4 => sprites.ReadOam(oamAddr),
            7 => bg.PeekData(),
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
            case 4: sprites.WriteOam(oamAddr, value); oamAddr++; break;
            case 5: bg.WriteScroll(value); break;
            case 6: bg.WriteAddr(value); break;
            case 7: bg.WriteData(ctrl, value); break;
        }
    }

    public void WriteOam(int index, byte value) => sprites.WriteOam(index, value);
    public void ClearPendingNmi() => nmiPending = false;

    private byte ReadStatus()
    {
        if (scanline == VBlankStartScanline && cycle > 0 && cycle <= 3)
        {
            nmiSuppressed = true;
            vblankSuppressed = true;
        }
        byte result = PeekStatus();
        bg.ClearWriteToggle();
        status &= 0x7F;
        return result;
    }

    private byte PeekStatus() => (byte)(status & 0xE0);

    private void WriteCtrl(byte value)
    {
        bool prevNmiEnable = (ctrl & CtrlNmiEnable) != 0;
        bool nextNmiEnable = (value & CtrlNmiEnable) != 0;
        ctrl = value;
        bg.WriteCtrlNametable(value);

        if (prevNmiEnable && !nextNmiEnable && InVblankNmiSuppressionWindow)
            nmiDelay = 0;

        if (!prevNmiEnable && NmiOutput && !InVblankClearNmiEnableRace)
            nmiDelay = NmiDelayCycles;
    }

    private void SkipOddFrameCycle()
    {
        cycle = 0;
        scanline = 0;
        evenFrame = !evenFrame;
    }
}
