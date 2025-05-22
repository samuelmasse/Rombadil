namespace Rombadil.Cpu.Emulator;

public struct CpuRegisters
{
    public ushort PC;
    public byte AC;
    public byte X;
    public byte Y;
    public CpuStatus SR;
    public byte SP;
}
