namespace Rombadil.Nes.Emulator;

public class NesMapper
{
    private readonly NesBatteryRam batteryRam;
    private readonly NesCartridgeRam prgRam;
    private readonly NesCartridgeRam chrRam;
    protected bool irqPending;
    protected NesMirroring mirroring;

    public NesMapper() : this(default) { }

    protected NesMapper(NesCartridgeRamSizes sizes)
    {
        batteryRam = new NesBatteryRam(sizes.PrgNvRam + sizes.ChrNvRam);
        prgRam = new NesCartridgeRam(sizes.PrgRam, sizes.PrgNvRam, batteryRam, 0);
        chrRam = new NesCartridgeRam(sizes.ChrRam, sizes.ChrNvRam, batteryRam, sizes.PrgNvRam);
    }

    public virtual bool PendingIrq => irqPending;
    public int BatterySaveSize => batteryRam.Length;
    public bool BatterySaveDirty => batteryRam.Dirty;

    public virtual void ClearPendingIrq() => irqPending = false;

    public virtual void Write(ushort addr, byte value) { }
    public virtual byte Read(ushort addr) => 0;
    public virtual void WriteExpansion(ushort addr, byte value) { }
    public virtual byte ReadExpansion(ushort addr) => 0;
    public virtual byte PeekExpansion(ushort addr) => ReadExpansion(addr);
    public virtual void WritePrgRam(ushort addr, byte value) => WritePrgRamOffset(addr - 0x6000, value);
    public virtual byte ReadPrgRam(ushort addr) => ReadPrgRamOffset(addr - 0x6000);
    public virtual byte PeekPrgRam(ushort addr) => ReadPrgRam(addr);
    public virtual void WritePrgRom(ushort addr, byte value) => Write(addr, value);
    public virtual byte ReadPrgRom(ushort addr) => Read(addr);
    public virtual byte PeekPrgRom(ushort addr) => ReadPrgRom(addr);
    public virtual void WriteChr(ushort addr, byte value) { }
    public virtual byte ReadChr(ushort addr) => 0;
    public virtual byte ReadChrBg(ushort addr, ushort ntAddr) => ReadChr(addr);
    public virtual byte ReadChrSprite(ushort addr, bool is8x16) => ReadChr(addr);
    public virtual byte ReadBgAttribute(ushort ntAddr, byte defaultAttr) => defaultAttr;
    public virtual void NotifyPpuCtrl(byte value) { }
    public virtual void NotifyPpuMask(byte value) { }
    public virtual void ResetAudio() { }
    public virtual void StepAudio() { }
    public virtual float SampleAudio() => 0;
    public virtual void StepCpuCycle() { }
    public virtual void ClockIrq() { }
    public virtual void NotifyScanline(int scanline) { }

    public void LoadBatterySave(ReadOnlySpan<byte> source) => batteryRam.Load(source);
    public void CopyBatterySave(Span<byte> destination) => batteryRam.CopyTo(destination);
    public void MarkBatterySaveClean() => batteryRam.MarkClean();

    private protected int PrgRamLength => prgRam.Length;
    private protected byte ReadPrgRamOffset(int offset) => prgRam.Read(offset);
    private protected void WritePrgRamOffset(int offset, byte value) => prgRam.Write(offset, value);
    private protected byte ReadChrRam(ushort addr) => chrRam.Read(addr);
    private protected void WriteChrRam(ushort addr, byte value) => chrRam.Write(addr, value);

    public virtual byte ReadNametable(byte[] vram, ushort addr) => vram[MapNametableAddr(addr)];
    public virtual void WriteNametable(byte[] vram, ushort addr, byte value) => vram[MapNametableAddr(addr)] = value;

    public virtual int MapNametableAddr(ushort addr)
    {
        int index = addr - 0x2000;

        return mirroring switch
        {
            NesMirroring.Horizontal => ((index & 0x800) >> 1) | (index & 0x3FF),
            NesMirroring.Vertical => index & 0x7FF,
            NesMirroring.SingleScreenLow => index & 0x3FF,
            NesMirroring.SingleScreenHigh => 0x400 | (index & 0x3FF),
            NesMirroring.FourScreen => 0x800 | (index & 0xFFF),
            _ => index & 0x7FF,
        };
    }
}
