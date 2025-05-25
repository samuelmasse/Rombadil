namespace Rombadil.Cpu.Emulator;

public struct CpuEmulatorRegisters
{
    public ushort PC;
    public byte AC;
    public byte X;
    public byte Y;
    public CpuStatus SR;
    public byte SP;
}
