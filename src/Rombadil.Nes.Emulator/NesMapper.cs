namespace Rombadil.Nes.Emulator;

public class NesMapper
{
    protected bool irqPending;
    protected NesMirroring mirroring;

    public bool PendingIrq => irqPending;

    public void ClearPendingIrq() => irqPending = false;

    public virtual void Write(ushort addr, byte value) { }
    public virtual byte Read(ushort addr) => 0;
    public virtual void WriteChr(ushort addr, byte value) { }
    public virtual byte ReadChr(ushort addr) => 0;
    public virtual void ClockIrq() { }

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
