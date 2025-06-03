namespace Rombadil.Nes.Emulator;

public class NesMapperMmc1(Memory<byte> prg, Memory<byte> chr) : NesMapper
{
    private readonly byte[] chrRam = new byte[0x2000];
    private byte shift = 0x10;
    private byte control = 0x0C;
    private byte chrBank0;
    private byte chrBank1;
    private byte prgBank;

    public override void Write(ushort addr, byte value)
    {
        if ((value & 0x80) != 0)
        {
            shift = 0x10;
            control |= 0x0C;
            return;
        }

        bool complete = (shift & 1) != 0;
        shift = (byte)((shift >> 1) | ((value & 1) << 4));

        if (complete)
        {
            int reg = (addr >> 13) & 0b11;

            switch (reg)
            {
                case 0: control = shift; break;
                case 1: chrBank0 = shift; break;
                case 2: chrBank1 = shift; break;
                case 3: prgBank = shift; break;
            }

            shift = 0x10;
        }
    }

    public override byte Read(ushort addr)
    {
        int mode = (control >> 2) & 0b11;

        if (mode is 0 or 1)
        {
            int bank = (prgBank & 0x0E) * 0x4000;
            int index = bank + (addr - 0x8000);
            return prg.Span[index % prg.Length];
        }
        else if (mode == 2)
        {
            if (addr < 0xC000)
                return prg.Span[addr - 0x8000];
            else
            {
                int bank = prgBank * 0x4000;
                int index = bank + (addr - 0xC000);
                return prg.Span[index % prg.Length];
            }
        }
        else
        {
            if (addr < 0xC000)
            {
                int bank = prgBank * 0x4000;
                int index = bank + (addr - 0x8000);
                return prg.Span[index % prg.Length];
            }
            else
            {
                int bank = prg.Length - 0x4000;
                return prg.Span[bank + (addr - 0xC000)];
            }
        }
    }

    public override void WriteChr(ushort addr, byte value)
    {
        if (chr.Length == 0)
            chrRam[addr & 0x1FFF] = value;
    }

    public override byte ReadChr(ushort addr)
    {
        if (chr.Length == 0)
            return chrRam[addr & 0x1FFF];

        if ((control & 0x10) != 0)
        {
            if (addr < 0x1000)
            {
                int bank = chrBank0 * 0x1000;
                return chr.Span[(bank + addr) % chr.Length];
            }
            else
            {
                int bank = chrBank1 * 0x1000;
                return chr.Span[(bank + (addr - 0x1000)) % chr.Length];
            }
        }
        else
        {
            int bank = (chrBank0 & 0xFE) * 0x1000;
            return chr.Span[(bank + addr) % chr.Length];
        }
    }

    public override int MapNametableAddr(ushort addr)
    {
        int index = addr - 0x2000;
        int mode = control & 0b11;

        return mode switch
        {
            0 => index % 0x400,
            1 => 0x400 + (index % 0x400),
            2 => index % 0x800,
            3 => ((index & 0x800) >> 1) | (index & 0x3FF),
            _ => index % 0x800
        };
    }
}
