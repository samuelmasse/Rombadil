namespace Rombadil.Cpu.Emulator;

public enum CpuEmulatorIllegalOpcode : byte
{
    NOP_1A = 0x1A,
    NOP_3A = 0x3A,
    NOP_5A = 0x5A,
    NOP_7A = 0x7A,
    NOP_DA = 0xDA,
    NOP_FA = 0xFA,

    NOP_IM_80 = 0x80,
    NOP_IM_82 = 0x82,
    NOP_IM_89 = 0x89,
    NOP_IM_C2 = 0xC2,
    NOP_IM_E2 = 0xE2,

    NOP_ZP_04 = 0x04,
    NOP_ZP_44 = 0x44,
    NOP_ZP_64 = 0x64,

    NOP_ZPX_14 = 0x14,
    NOP_ZPX_34 = 0x34,
    NOP_ZPX_54 = 0x54,
    NOP_ZPX_74 = 0x74,
    NOP_ZPX_D4 = 0xD4,
    NOP_ZPX_F4 = 0xF4,

    NOP_ABS_0C = 0x0C,

    NOP_ABSX_1C = 0x1C,
    NOP_ABSX_3C = 0x3C,
    NOP_ABSX_5C = 0x5C,
    NOP_ABSX_7C = 0x7C,
    NOP_ABSX_DC = 0xDC,
    NOP_ABSX_FC = 0xFC,

    LAX_ZP = 0xA7,
    LAX_ZPY = 0xB7,
    LAX_ABS = 0xAF,
    LAX_ABSY = 0xBF,
    LAX_INDX = 0xA3,
    LAX_INDY = 0xB3,

    SAX_ZP = 0x87,
    SAX_ZPY = 0x97,
    SAX_ABS = 0x8F,
    SAX_INDX = 0x83,

    SBC_IM_EB = 0xEB,

    DCP_ZP = 0xC7,
    DCP_ZPX = 0xD7,
    DCP_ABS = 0xCF,
    DCP_ABSX = 0xDF,
    DCP_ABSY = 0xDB,
    DCP_INDX = 0xC3,
    DCP_INDY = 0xD3,

    ISB_ZP = 0xE7,
    ISB_ZPX = 0xF7,
    ISB_ABS = 0xEF,
    ISB_ABSX = 0xFF,
    ISB_ABSY = 0xFB,
    ISB_INDX = 0xE3,
    ISB_INDY = 0xF3,

    SLO_ZP = 0x07,
    SLO_ZPX = 0x17,
    SLO_ABS = 0x0F,
    SLO_ABSX = 0x1F,
    SLO_ABSY = 0x1B,
    SLO_INDX = 0x03,
    SLO_INDY = 0x13,

    RLA_ZP = 0x27,
    RLA_ZPX = 0x37,
    RLA_ABS = 0x2F,
    RLA_ABSX = 0x3F,
    RLA_ABSY = 0x3B,
    RLA_INDX = 0x23,
    RLA_INDY = 0x33,

    SRE_ZP = 0x47,
    SRE_ZPX = 0x57,
    SRE_ABS = 0x4F,
    SRE_ABSX = 0x5F,
    SRE_ABSY = 0x5B,
    SRE_INDX = 0x43,
    SRE_INDY = 0x53,

    RRA_ZP = 0x67,
    RRA_ZPX = 0x77,
    RRA_ABS = 0x6F,
    RRA_ABSX = 0x7F,
    RRA_ABSY = 0x7B,
    RRA_INDX = 0x63,
    RRA_INDY = 0x73,
}
