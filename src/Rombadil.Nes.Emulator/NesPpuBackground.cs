namespace Rombadil.Nes.Emulator;

public class NesPpuBackground(NesPpuMemory memory, NesMapper mapper)
{
    private const byte CtrlVramIncrement = 0x04;
    private const byte CtrlBgPatternTable = 0x10;

    private ushort v;
    private ushort t;
    private byte fineX;
    private bool writeToggle;
    private byte readBuffer;
    private byte nametableLatch;
    private byte attributeLatch;
    private byte bgPatternLowLatch;
    private byte bgPatternHighLatch;
    private ushort bgPatternLowShift;
    private ushort bgPatternHighShift;
    private byte attributeLowShift;
    private byte attributeHighShift;
    private bool attributeLowLatch;
    private bool attributeHighLatch;

    public ushort Address => v;

    public void Reset()
    {
        v = 0;
        t = 0;
        fineX = 0;
        writeToggle = false;
        readBuffer = 0;
        nametableLatch = 0;
        attributeLatch = 0;
        bgPatternLowLatch = 0;
        bgPatternHighLatch = 0;
        bgPatternLowShift = 0;
        bgPatternHighShift = 0;
        attributeLowShift = 0;
        attributeHighShift = 0;
        attributeLowLatch = false;
        attributeHighLatch = false;
    }

    public void RunPipeline(int cycle, bool prerenderLine, byte ctrl)
    {
        bool inActiveFetch = cycle >= 1 && cycle <= 256;
        bool inPrefetch = cycle >= 321 && cycle <= 336;

        if (inActiveFetch || inPrefetch)
        {
            ShiftRegisters();

            switch ((cycle - 1) & 7)
            {
                case 0: FetchNametableByte(); break;
                case 2: FetchAttributeByte(); break;
                case 4: FetchPatternLow(ctrl); break;
                case 6: FetchPatternHigh(ctrl); break;
                case 7: ReloadShifters(); IncrementCoarseX(); break;
            }
        }

        if (cycle == 256)
            IncrementY();

        if (cycle == 257)
            CopyHorizontalFromT();

        if (prerenderLine && cycle >= 280 && cycle <= 304)
            CopyVerticalFromT();
    }

    public (int pixel, int palette) SamplePixel()
    {
        int bit = 15 - fineX;
        int pixel = ((bgPatternHighShift >> bit) & 1) << 1
                  | ((bgPatternLowShift >> bit) & 1);
        int attrBit = 7 - fineX;
        int palette = ((attributeHighShift >> attrBit) & 1) << 1
                    | ((attributeLowShift >> attrBit) & 1);
        return (pixel, palette);
    }

    public void WriteCtrlNametable(byte value) => t = (ushort)((t & 0xF3FF) | ((value & 0x03) << 10));

    public void WriteScroll(byte value)
    {
        if (!writeToggle)
        {
            t = (ushort)((t & 0xFFE0) | (value >> 3));
            fineX = (byte)(value & 0x07);
        }
        else
        {
            t = (ushort)((t & 0x8FFF) | ((value & 0x07) << 12));
            t = (ushort)((t & 0xFC1F) | ((value & 0xF8) << 2));
        }
        writeToggle = !writeToggle;
    }

    public void WriteAddr(byte value)
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

    public void ClearWriteToggle() => writeToggle = false;

    public byte ReadData(byte ctrl)
    {
        byte result;
        if (v < 0x3F00)
        {
            result = readBuffer;
            readBuffer = memory.Read(v);
        }
        else
        {
            result = memory.Read(v);
            readBuffer = memory.Read((ushort)(v - 0x1000));
        }
        v += (ctrl & CtrlVramIncrement) != 0 ? (ushort)32 : (ushort)1;
        return result;
    }

    public byte PeekData()
    {
        if (v < 0x3F00)
            return readBuffer;
        return memory.Read(v);
    }

    public void WriteData(byte ctrl, byte value)
    {
        memory.Write(v, value);
        v += (ctrl & CtrlVramIncrement) != 0 ? (ushort)32 : (ushort)1;
    }

    private void FetchNametableByte() => nametableLatch = memory.Read((ushort)(0x2000 | (v & 0x0FFF)));

    private void FetchAttributeByte()
    {
        ushort addr = (ushort)(
            0x23C0
            | (v & 0x0C00)
            | ((v >> 4) & 0x38)
            | ((v >> 2) & 0x07));
        attributeLatch = memory.Read(addr);
    }

    private void FetchPatternLow(byte ctrl)
    {
        int patternTable = (ctrl & CtrlBgPatternTable) != 0 ? 0x1000 : 0;
        int fineY = (v >> 12) & 7;
        bgPatternLowLatch = mapper.ReadChr((ushort)(patternTable + nametableLatch * 16 + fineY));
    }

    private void FetchPatternHigh(byte ctrl)
    {
        int patternTable = (ctrl & CtrlBgPatternTable) != 0 ? 0x1000 : 0;
        int fineY = (v >> 12) & 7;
        bgPatternHighLatch = mapper.ReadChr((ushort)(patternTable + nametableLatch * 16 + fineY + 8));
    }

    private void ShiftRegisters()
    {
        bgPatternLowShift <<= 1;
        bgPatternHighShift <<= 1;
        attributeLowShift = (byte)((attributeLowShift << 1) | (attributeLowLatch ? 1 : 0));
        attributeHighShift = (byte)((attributeHighShift << 1) | (attributeHighLatch ? 1 : 0));
    }

    private void ReloadShifters()
    {
        bgPatternLowShift = (ushort)((bgPatternLowShift & 0xFF00) | bgPatternLowLatch);
        bgPatternHighShift = (ushort)((bgPatternHighShift & 0xFF00) | bgPatternHighLatch);
        int attributeShift = ((v >> 4) & 4) | (v & 2);
        int paletteIndex = (attributeLatch >> attributeShift) & 3;
        attributeLowLatch = (paletteIndex & 1) != 0;
        attributeHighLatch = (paletteIndex & 2) != 0;
    }

    private void IncrementCoarseX()
    {
        if ((v & 0x001F) == 31)
        {
            v &= 0xFFE0;
            v ^= 0x0400;
        }
        else
            v++;
    }

    private void IncrementY()
    {
        if ((v & 0x7000) != 0x7000)
        {
            v += 0x1000;
            return;
        }

        v &= 0x8FFF;
        int y = (v & 0x03E0) >> 5;
        if (y == 29)
        {
            y = 0;
            v ^= 0x0800;
        }
        else if (y == 31)
            y = 0;
        else
            y++;
        v = (ushort)((v & 0xFC1F) | (y << 5));
    }

    private void CopyHorizontalFromT() => v = (ushort)((v & 0xFBE0) | (t & 0x041F));
    private void CopyVerticalFromT() => v = (ushort)((v & 0x841F) | (t & 0x7BE0));
}
