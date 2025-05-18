namespace Rombadil.Cpu;

public enum CpuOpcode : byte
{
    LDA_IM = 0xA9,
    STA_ABS = 0x8D,
    TAX = 0xAA,
    INX = 0xE8,
    DEX = 0xCA,
    BEQ = 0xF0,
    CLC = 0x18,
    ADC_IM = 0x69,
    JSR = 0x20,
    RTS = 0x60,
}
