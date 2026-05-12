namespace Rombadil.Nes.Emulator;

public class NesMapperMmc5 : NesMapper
{
    private readonly Memory<byte> prg;
    private readonly Memory<byte> chr;
    private readonly byte[] chrRam = new byte[0x2000];
    private readonly byte[] exRam = new byte[0x400];
    private readonly NesMmc5Pulse audioPulse1 = new();
    private readonly NesMmc5Pulse audioPulse2 = new();

    private byte prgMode = 3;
    private byte chrMode = 3;
    private readonly byte[] prgBank = new byte[4];
    private readonly int[] chrBankA = new int[8];
    private readonly int[] chrBankB = new int[4];
    private byte chrUpper;
    private byte prgRamBank;
    private byte ntMapping;
    private byte fillTile;
    private byte fillAttr;
    private bool chrIoUsesBankB;
    private bool sprite8x16;
    private bool substitutionsEnabled;

    private int scanlineCounter;
    private byte scanlineTarget;
    private bool irqEnable;
    private bool irqInternalPending;
    private bool inFrame;

    private byte exRamMode;
    private byte mulA;
    private byte mulB;
    private bool pcmReadMode;
    private bool pcmIrqEnable;
    private bool pcmIrqPending;
    private byte pcmDac;
    private long audioCycles;
    private int audioFrameCycle = 1;

    private bool IndependentChrActive => sprite8x16 && substitutionsEnabled;
    private bool ExtendedAttributesActive => exRamMode == 1 && substitutionsEnabled;
    private bool PcmIrqLine => pcmIrqPending && pcmIrqEnable;
    public override bool PendingIrq => irqPending || PcmIrqLine;

    public NesMapperMmc5(Memory<byte> prg, Memory<byte> chr, NesMirroring mirroring)
    {
        this.prg = prg;
        this.chr = chr;
        this.mirroring = mirroring;

        int last8KBank = Math.Max(0, prg.Length / 0x2000 - 1);
        prgBank[3] = (byte)(0x80 | (last8KBank & 0x7F));
    }

    public override void Write(ushort addr, byte value)
    {
        if (addr >= 0x5C00 && addr <= 0x5FFF)
        {
            exRam[addr - 0x5C00] = value;
            return;
        }

        switch (addr)
        {
            case >= 0x5000 and <= 0x5015: WriteAudioRegister(addr, value); break;
            case 0x5100: prgMode = (byte)(value & 0x03); break;
            case 0x5101: chrMode = (byte)(value & 0x03); break;
            case 0x5104: exRamMode = (byte)(value & 0x03); break;
            case 0x5105: ntMapping = value; break;
            case 0x5106: fillTile = value; break;
            case 0x5107: fillAttr = (byte)(value & 0x03); break;
            case 0x5113: prgRamBank = value; break;
            case 0x5203: scanlineTarget = value; break;
            case 0x5204:
                irqEnable = (value & 0x80) != 0;
                irqPending = irqEnable && irqInternalPending;
                break;
            case 0x5205: mulA = value; break;
            case 0x5206: mulB = value; break;
            case 0x5114: prgBank[0] = value; break;
            case 0x5115: prgBank[1] = value; break;
            case 0x5116: prgBank[2] = value; break;
            case 0x5117: prgBank[3] = (byte)(value | 0x80); break;
            case 0x5130: chrUpper = (byte)(value & 0x03); break;
            default:
                if (addr >= 0x5120 && addr <= 0x5127)
                {
                    chrBankA[addr - 0x5120] = value | (chrUpper << 8);
                    chrIoUsesBankB = false;
                }
                else if (addr >= 0x5128 && addr <= 0x512B)
                {
                    chrBankB[addr - 0x5128] = value | (chrUpper << 8);
                    chrIoUsesBankB = true;
                }
                break;
        }
    }

    public override byte Read(ushort addr)
    {
        if (addr == 0x5010) return ReadPcmMode();
        if (addr == 0x5015) return ReadAudioStatus();

        if (addr == 0x5204)
        {
            byte result = (byte)((irqInternalPending ? 0x80 : 0) | (inFrame ? 0x40 : 0));
            irqInternalPending = false;
            irqPending = false;
            return result;
        }

        if (addr == 0x5205) return (byte)(mulA * mulB);
        if (addr == 0x5206) return (byte)((mulA * mulB) >> 8);

        if (addr >= 0x5C00 && addr <= 0x5FFF)
            return exRam[addr - 0x5C00];

        if (addr >= 0x8000)
            return ReadPrg(addr);

        return 0;
    }

    public override void ClearPendingIrq() => irqPending = false;

    public override void ResetAudio()
    {
        audioPulse1.Reset();
        audioPulse2.Reset();
        pcmReadMode = false;
        pcmIrqEnable = false;
        pcmIrqPending = false;
        pcmDac = 0;
        audioCycles = 0;
        audioFrameCycle = 1;
    }

    public override void StepAudio()
    {
        if ((audioCycles & 1) == 0)
        {
            if (audioFrameCycle == 14915)
                audioFrameCycle = 0;

            audioFrameCycle++;
        }
        else
        {
            if (audioFrameCycle is 3730 or 7458 or 11187 or 14915)
                ClockAudioFrame();

            audioPulse1.Step();
            audioPulse2.Step();
        }

        audioCycles++;
    }

    public override float SampleAudio()
    {
        float pulse1 = audioPulse1.Sample();
        float pulse2 = audioPulse2.Sample();
        float pulseMix = pulse1 + pulse2;
        float pulseOut = pulseMix == 0
            ? 0
            : 95.88f / ((8128f / pulseMix) + 100f);

        float pcmMix = pcmDac / 22638f;
        float pcmOut = pcmDac == 0
            ? 0
            : 159.79f / ((1f / pcmMix) + 100f);

        return pulseOut + pcmOut;
    }

    private void WriteAudioRegister(ushort addr, byte value)
    {
        if (addr >= 0x5000 && addr <= 0x5003)
        {
            audioPulse1.WriteRegister(addr - 0x5000, value);
            return;
        }

        if (addr >= 0x5004 && addr <= 0x5007)
        {
            audioPulse2.WriteRegister(addr - 0x5004, value);
            return;
        }

        switch (addr)
        {
            case 0x5010:
                pcmReadMode = (value & 0x01) != 0;
                pcmIrqEnable = (value & 0x80) != 0;
                break;
            case 0x5011:
                if (!pcmReadMode)
                    WritePcmDac(value);
                break;
            case 0x5015:
                audioPulse1.Toggle((value & 0x01) != 0);
                audioPulse2.Toggle((value & 0x02) != 0);
                break;
        }
    }

    private byte ReadPcmMode()
    {
        byte result = (byte)((PcmIrqLine ? 0x80 : 0) | (pcmReadMode ? 0x01 : 0));
        pcmIrqPending = false;
        return result;
    }

    private byte ReadAudioStatus()
    {
        byte result = 0;
        if (audioPulse1.Length > 0)
            result |= 0x01;
        if (audioPulse2.Length > 0)
            result |= 0x02;
        return result;
    }

    private void ClockAudioFrame()
    {
        audioPulse1.ClockEnvelope();
        audioPulse2.ClockEnvelope();
        audioPulse1.ClockLength();
        audioPulse2.ClockLength();
    }

    private void WritePcmDac(byte value)
    {
        if (value == 0)
        {
            pcmIrqPending = true;
            return;
        }

        pcmDac = value;
        pcmIrqPending = false;
    }

    public override void NotifyPpuCtrl(byte value) => sprite8x16 = (value & 0x20) != 0;
    public override void NotifyPpuMask(byte value)
    {
        bool nextSubstitutionsEnabled = (value & 0x18) != 0;
        if (!nextSubstitutionsEnabled || !substitutionsEnabled)
            ClearScanlineState();

        substitutionsEnabled = nextSubstitutionsEnabled;
    }

    public override void NotifyScanline(int scanline)
    {
        if (scanline < 240)
        {
            if (!inFrame)
            {
                inFrame = true;
                scanlineCounter = 0;
            }
            else
            {
                scanlineCounter = (scanlineCounter + 1) & 0xFF;
                if (scanlineTarget != 0 && scanlineCounter == scanlineTarget)
                    irqInternalPending = true;
            }
        }
        else if (scanline == 240)
        {
            ClearScanlineState();
        }

        irqPending = irqEnable && irqInternalPending;
    }

    private void ClearScanlineState()
    {
        scanlineCounter = 0;
        inFrame = false;
        irqInternalPending = false;
        irqPending = false;
    }

    private byte ReadPrg(ushort addr)
    {
        int window = (addr - 0x8000) >> 13;
        int offset = addr & 0x1FFF;

        int bankIdx;
        bool isRom;

        switch (prgMode)
        {
            case 0:
                bankIdx = (prgBank[3] & 0x7C) | (window & 0x03);
                isRom = true;
                break;
            case 1:
                if (window < 2)
                {
                    bankIdx = (prgBank[1] & 0x7E) | (window & 1);
                    isRom = (prgBank[1] & 0x80) != 0;
                }
                else
                {
                    bankIdx = (prgBank[3] & 0x7E) | (window & 1);
                    isRom = true;
                }
                break;
            case 2:
                if (window < 2)
                {
                    bankIdx = (prgBank[1] & 0x7E) | (window & 1);
                    isRom = (prgBank[1] & 0x80) != 0;
                }
                else if (window == 2)
                {
                    bankIdx = prgBank[2] & 0x7F;
                    isRom = (prgBank[2] & 0x80) != 0;
                }
                else
                {
                    bankIdx = prgBank[3] & 0x7F;
                    isRom = true;
                }
                break;
            default:
                byte reg = prgBank[window];
                bankIdx = reg & 0x7F;
                isRom = window == 3 || (reg & 0x80) != 0;
                break;
        }

        if (!isRom)
            return 0;

        byte result = prg.Span[(bankIdx * 0x2000 + offset) % prg.Length];
        if (pcmReadMode && addr <= 0xBFFF)
            WritePcmDac(result);

        return result;
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        if (sprite8x16 && chrIoUsesBankB)
            return ReadChrFromBankSetB(addr);

        return ReadChrFromBankSetA(addr);
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    public override byte ReadChrBg(ushort addr, ushort ntAddr)
    {
        if (ExtendedAttributesActive && chr.Length > 0)
        {
            byte exByte = exRam[ntAddr & 0x3FF];
            int bank = (exByte & 0x3F) | (chrUpper << 6);
            return ReadChrBank(bank, 0x1000, addr & 0x0FFF);
        }

        return IndependentChrActive ? ReadChrFromBankSetB(addr) : ReadChrFromBankSetA(addr);
    }

    public override byte ReadBgAttribute(ushort ntAddr, byte defaultAttr)
    {
        if (ExtendedAttributesActive)
            return (byte)((exRam[ntAddr & 0x3FF] >> 6) * 0x55);

        return defaultAttr;
    }

    public override byte ReadChrSprite(ushort addr, bool is8x16) => ReadChrFromBankSetA(addr);

    private byte ReadChrFromBankSetA(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        int bank;
        int offsetInBank;
        int bankSize;

        switch (chrMode)
        {
            case 0:
                bank = chrBankA[7];
                bankSize = 0x2000;
                offsetInBank = addr & 0x1FFF;
                break;
            case 1:
                bank = addr < 0x1000 ? chrBankA[3] : chrBankA[7];
                bankSize = 0x1000;
                offsetInBank = addr & 0x0FFF;
                break;
            case 2:
                int slot2 = ((addr >> 11) & 3) * 2 + 1;
                bank = chrBankA[slot2];
                bankSize = 0x0800;
                offsetInBank = addr & 0x07FF;
                break;
            default:
                int slot3 = (addr >> 10) & 7;
                bank = chrBankA[slot3];
                bankSize = 0x0400;
                offsetInBank = addr & 0x03FF;
                break;
        }

        return ReadChrBank(bank, bankSize, offsetInBank);
    }

    private byte ReadChrFromBankSetB(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        int bank;
        int offsetInBank;
        int bankSize;

        switch (chrMode)
        {
            case 0:
                bank = chrBankB[3];
                bankSize = 0x2000;
                offsetInBank = addr & 0x1FFF;
                break;
            case 1:
                bank = chrBankB[3];
                bankSize = 0x1000;
                offsetInBank = addr & 0x0FFF;
                break;
            case 2:
                int slot2 = ((addr >> 11) & 1) * 2 + 1;
                bank = chrBankB[slot2];
                bankSize = 0x0800;
                offsetInBank = addr & 0x07FF;
                break;
            default:
                int slot3 = (addr >> 10) & 3;
                bank = chrBankB[slot3];
                bankSize = 0x0400;
                offsetInBank = addr & 0x03FF;
                break;
        }

        return ReadChrBank(bank, bankSize, offsetInBank);
    }

    private byte ReadChrBank(int bank, int bankSize, int offsetInBank)
    {
        int mappedAddr = (bank * bankSize + offsetInBank) % chr.Length;
        return chr.Span[mappedAddr];
    }

    public override byte ReadNametable(byte[] vram, ushort addr)
    {
        int ntIndex = (addr >> 10) & 3;
        int mode = (ntMapping >> (ntIndex * 2)) & 3;
        int offset = addr & 0x3FF;

        return mode switch
        {
            0 => vram[offset],
            1 => vram[0x400 + offset],
            2 => exRamMode <= 1 ? exRam[offset] : (byte)0,
            _ => offset < 0x3C0 ? fillTile : (byte)(fillAttr * 0x55),
        };
    }

    public override void WriteNametable(byte[] vram, ushort addr, byte value)
    {
        int ntIndex = (addr >> 10) & 3;
        int mode = (ntMapping >> (ntIndex * 2)) & 3;
        int offset = addr & 0x3FF;

        switch (mode)
        {
            case 0: vram[offset] = value; break;
            case 1: vram[0x400 + offset] = value; break;
            case 2:
                if (exRamMode <= 1)
                    exRam[offset] = value;
                break;
        }
    }
}
