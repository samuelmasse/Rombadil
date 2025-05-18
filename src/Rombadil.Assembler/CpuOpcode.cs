namespace Rombadil.Cpu;

public enum CpuOpcode : byte
{
    ADC_IM = 0x69,       // Immediate
    ADC_ZP = 0x65,       // Zero Page
    ADC_ZPX = 0x75,      // Zero Page,X
    ADC_ABS = 0x6D,      // Absolute
    ADC_ABSX = 0x7D,     // Absolute,X
    ADC_ABSY = 0x79,     // Absolute,Y
    ADC_INDX = 0x61,     // (Indirect,X)
    ADC_INDY = 0x71,    // (Indirect),Y

    LDA_IM = 0xA9,
    STA_ABS = 0x8D,
    TAX = 0xAA,
    INX = 0xE8,
    DEX = 0xCA,
    BEQ = 0xF0,
    CLC = 0x18,
    JSR = 0x20,
    RTS = 0x60,
}
