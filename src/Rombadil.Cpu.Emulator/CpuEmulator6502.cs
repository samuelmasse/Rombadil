namespace Rombadil.Cpu.Emulator;

public class CpuEmulator6502(Memory<byte> memory)
{
    private readonly CpuEmulatorState state = new(memory);

    public CpuEmulatorRegisters Reg => state.Reg;
    public long Cycles => state.Cycles;

    internal CpuEmulatorState State => state;

    public void Reset(ushort? pc = null)
    {
        state.PC = pc ?? (ushort)(state.Mem[0xFFFC] | (state.Mem[0xFFFD] << 8));

        state.AC = 0;
        state.X = 0;
        state.Y = 0;

        state.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        state.SP = 0xFD;

        state.Cycles = 7;
    }

    public void Step()
    {
        var b = state.Mem[state.PC++];

        if (CpuOpcodeMap.TryDecodeOpcode((CpuOpcode)b, out var decode))
            StepLegal(decode.Item1, decode.Item2);
        else if (CpuEmulatorIllegalOpcodeMap.TryDecodeOpcode((CpuEmulatorIllegalOpcode)b, out var illegal))
            StepIllegal(illegal.Item1, illegal.Item2);
        else throw new Exception();
    }

    private void StepLegal(CpuInstruction instruction, CpuAddressingMode mode)
    {
        var exec = new CpuEmulatorExecutor(state, mode);

        switch (instruction)
        {
            case CpuInstruction.ADC: exec.Adc(); break;
            case CpuInstruction.AND: exec.And(); break;
            case CpuInstruction.ASL: exec.Asl(); break;
            case CpuInstruction.BIT: exec.Bit(); break;
            case CpuInstruction.BPL: exec.Bpl(); break;
            case CpuInstruction.BMI: exec.Bmi(); break;
            case CpuInstruction.BVC: exec.Bvc(); break;
            case CpuInstruction.BVS: exec.Bvs(); break;
            case CpuInstruction.BCC: exec.Bcc(); break;
            case CpuInstruction.BCS: exec.Bcs(); break;
            case CpuInstruction.BNE: exec.Bne(); break;
            case CpuInstruction.BEQ: exec.Beq(); break;
            case CpuInstruction.BRK: exec.Brk(); break;
            case CpuInstruction.CMP: exec.Cmp(); break;
            case CpuInstruction.CPX: exec.Cpx(); break;
            case CpuInstruction.CPY: exec.Cpy(); break;
            case CpuInstruction.DEC: exec.Dec(); break;
            case CpuInstruction.EOR: exec.Eor(); break;
            case CpuInstruction.CLC: exec.Clc(); break;
            case CpuInstruction.SEC: exec.Sec(); break;
            case CpuInstruction.CLI: exec.Cli(); break;
            case CpuInstruction.SEI: exec.Sei(); break;
            case CpuInstruction.CLV: exec.Clv(); break;
            case CpuInstruction.CLD: exec.Cld(); break;
            case CpuInstruction.SED: exec.Sed(); break;
            case CpuInstruction.INC: exec.Inc(); break;
            case CpuInstruction.JMP: exec.Jmp(); break;
            case CpuInstruction.JSR: exec.Jsr(); break;
            case CpuInstruction.LDA: exec.Lda(); break;
            case CpuInstruction.LDX: exec.Ldx(); break;
            case CpuInstruction.LDY: exec.Ldy(); break;
            case CpuInstruction.LSR: exec.Lsr(); break;
            case CpuInstruction.NOP: exec.Nop(); break;
            case CpuInstruction.ORA: exec.Ora(); break;
            case CpuInstruction.TAX: exec.Tax(); break;
            case CpuInstruction.TXA: exec.Txa(); break;
            case CpuInstruction.DEX: exec.Dex(); break;
            case CpuInstruction.INX: exec.Inx(); break;
            case CpuInstruction.TAY: exec.Tay(); break;
            case CpuInstruction.TYA: exec.Tya(); break;
            case CpuInstruction.DEY: exec.Dey(); break;
            case CpuInstruction.INY: exec.Iny(); break;
            case CpuInstruction.ROL: exec.Rol(); break;
            case CpuInstruction.ROR: exec.Ror(); break;
            case CpuInstruction.RTI: exec.Rti(); break;
            case CpuInstruction.RTS: exec.Rts(); break;
            case CpuInstruction.SBC: exec.Sbc(); break;
            case CpuInstruction.STA: exec.Sta(); break;
            case CpuInstruction.TXS: exec.Txs(); break;
            case CpuInstruction.TSX: exec.Tsx(); break;
            case CpuInstruction.PHA: exec.Pha(); break;
            case CpuInstruction.PLA: exec.Pla(); break;
            case CpuInstruction.PHP: exec.Php(); break;
            case CpuInstruction.PLP: exec.Plp(); break;
            case CpuInstruction.STX: exec.Stx(); break;
            case CpuInstruction.STY: exec.Sty(); break;
        }
    }

    private void StepIllegal(CpuEmulatorIllegalInstruction instruction, CpuAddressingMode mode)
    {
        var illegalExec = new CpuEmulatorIllegalExecutor(state, mode);

        switch (instruction)
        {
            case CpuEmulatorIllegalInstruction.NOP: illegalExec.Nop(mode); break;
            case CpuEmulatorIllegalInstruction.LAX: illegalExec.Lax(mode); break;
            case CpuEmulatorIllegalInstruction.SAX: illegalExec.Sax(mode); break;
            case CpuEmulatorIllegalInstruction.SBC: illegalExec.Sbc(); break;
            case CpuEmulatorIllegalInstruction.DCP: illegalExec.Dcp(mode); break;
            case CpuEmulatorIllegalInstruction.ISB: illegalExec.Isb(mode); break;
            case CpuEmulatorIllegalInstruction.SLO: illegalExec.Slo(mode); break;
            case CpuEmulatorIllegalInstruction.RLA: illegalExec.Rla(mode); break;
            case CpuEmulatorIllegalInstruction.SRE: illegalExec.Sre(mode); break;
            case CpuEmulatorIllegalInstruction.RRA: illegalExec.Rra(mode); break;
        }
    }
}

