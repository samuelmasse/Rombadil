namespace Rombadil.Nes.Emulator;

public class NesMapper
{
    protected bool irqPending;

    public bool PendingIrq => irqPending;

    public void ClearPendingIrq() => irqPending = false;

    public virtual void Write(ushort addr, byte value) { }
    public virtual byte Read(ushort addr) => 0;
    public virtual void WriteChr(ushort addr, byte value) { }
    public virtual byte ReadChr(ushort addr) => 0;
    public virtual int MapNametableAddr(ushort addr) => (addr - 0x2000) % 0x800;
    public virtual void ClockIrq() { }
}
