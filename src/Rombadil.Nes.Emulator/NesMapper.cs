namespace Rombadil.Nes.Emulator;

public class NesMapper
{
    public virtual void Write(ushort addr, byte value) { }
    public virtual byte Read(ushort addr) => 0;
    public virtual void WriteChr(ushort addr, byte value) { }
    public virtual byte ReadChr(ushort addr) => 0;
}
