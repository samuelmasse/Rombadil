namespace Rombadil.Cpu.Emulator;

public class CpuEmulator6502
{
    private readonly CpuEmulatorState state;
    private readonly CpuEmulatorExecutor exec;
    private readonly CpuEmulatorIllegalExecutor illegalExec;

    public CpuEmulatorRegisters Reg => state.Reg;
    public long Cycles => state.Cycles;

    internal CpuEmulatorState State => state;

    public CpuEmulator6502(Memory<byte> memory)
    {
        state = new(memory);
        exec = new(state);
        illegalExec = new(state, exec);
    }

    public void Reset(ushort? pc = null)
    {
        state.Reg.PC = pc ?? (ushort)(state.Mem[0xFFFC] | (state.Mem[0xFFFD] << 8));

        state.Reg.AC = 0;
        state.Reg.X = 0;
        state.Reg.Y = 0;

        state.Reg.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        state.Reg.SP = 0xFD;

        state.Cycles = 7;
    }

    public void Step()
    {
        var b = state.Mem[state.Reg.PC++];

        if (CpuOpcodeMap.TryDecodeOpcode((CpuOpcode)b, out var decode))
            StepLegal(decode.Item1, decode.Item2);
        else if (CpuEmulatorIllegalOpcodeMap.TryDecodeOpcode((CpuEmulatorIllegalOpcode)b, out var illegal))
            StepIllegal(illegal.Item1, illegal.Item2);
        else throw new Exception();
    }

    private void StepLegal(CpuInstruction instruction, CpuAddressingMode mode)
    {
        switch (instruction)
        {
            case CpuInstruction.ADC: exec.Adc(mode); break;
            case CpuInstruction.AND: exec.And(mode); break;
            case CpuInstruction.ASL: exec.Asl(mode); break;
            case CpuInstruction.BIT: exec.Bit(mode); break;
            case CpuInstruction.BPL: exec.Bpl(); break;
            case CpuInstruction.BMI: exec.Bmi(); break;
            case CpuInstruction.BVC: exec.Bvc(); break;
            case CpuInstruction.BVS: exec.Bvs(); break;
            case CpuInstruction.BCC: exec.Bcc(); break;
            case CpuInstruction.BCS: exec.Bcs(); break;
            case CpuInstruction.BNE: exec.Bne(); break;
            case CpuInstruction.BEQ: exec.Beq(); break;
            case CpuInstruction.BRK: exec.Brk(); break;
            case CpuInstruction.CMP: exec.Cmp(mode); break;
            case CpuInstruction.CPX: exec.Cpx(mode); break;
            case CpuInstruction.CPY: exec.Cpy(mode); break;
            case CpuInstruction.DEC: exec.Dec(mode); break;
            case CpuInstruction.EOR: exec.Eor(mode); break;
            case CpuInstruction.CLC: exec.Clc(); break;
            case CpuInstruction.SEC: exec.Sec(); break;
            case CpuInstruction.CLI: exec.Cli(); break;
            case CpuInstruction.SEI: exec.Sei(); break;
            case CpuInstruction.CLV: exec.Clv(); break;
            case CpuInstruction.CLD: exec.Cld(); break;
            case CpuInstruction.SED: exec.Sed(); break;
            case CpuInstruction.INC: exec.Inc(mode); break;
            case CpuInstruction.JMP: exec.Jmp(mode); break;
            case CpuInstruction.JSR: exec.Jsr(); break;
            case CpuInstruction.LDA: exec.Lda(mode); break;
            case CpuInstruction.LDX: exec.Ldx(mode); break;
            case CpuInstruction.LDY: exec.Ldy(mode); break;
            case CpuInstruction.LSR: exec.Lsr(mode); break;
            case CpuInstruction.NOP: exec.Nop(); break;
            case CpuInstruction.ORA: exec.Ora(mode); break;
            case CpuInstruction.TAX: exec.Tax(); break;
            case CpuInstruction.TXA: exec.Txa(); break;
            case CpuInstruction.DEX: exec.Dex(); break;
            case CpuInstruction.INX: exec.Inx(); break;
            case CpuInstruction.TAY: exec.Tay(); break;
            case CpuInstruction.TYA: exec.Tya(); break;
            case CpuInstruction.DEY: exec.Dey(); break;
            case CpuInstruction.INY: exec.Iny(); break;
            case CpuInstruction.ROL: exec.Rol(mode); break;
            case CpuInstruction.ROR: exec.Ror(mode); break;
            case CpuInstruction.RTI: exec.Rti(); break;
            case CpuInstruction.RTS: exec.Rts(); break;
            case CpuInstruction.SBC: exec.Sbc(mode); break;
            case CpuInstruction.STA: exec.Sta(mode); break;
            case CpuInstruction.TXS: exec.Txs(); break;
            case CpuInstruction.TSX: exec.Tsx(); break;
            case CpuInstruction.PHA: exec.Pha(); break;
            case CpuInstruction.PLA: exec.Pla(); break;
            case CpuInstruction.PHP: exec.Php(); break;
            case CpuInstruction.PLP: exec.Plp(); break;
            case CpuInstruction.STX: exec.Stx(mode); break;
            case CpuInstruction.STY: exec.Sty(mode); break;
        }
    }

    private void StepIllegal(CpuEmulatorIllegalInstruction instruction, CpuAddressingMode mode)
    {
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

