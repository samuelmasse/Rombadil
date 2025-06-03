namespace Rombadil.Nes.Emulator;

public class NesMapper
{
    public virtual void Write(ushort addr, byte value) { }
    public virtual byte Read(ushort addr) => 0;
}
