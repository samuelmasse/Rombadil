namespace Rombadil.Cpu.Emulator;

public class CpuEmulator6502
{
    private readonly CpuEmulatorState state;
    private readonly CpuEmulatorExecutor exec;

    public CpuEmulatorRegisters Reg => state.Reg;
    public long Cycles => state.Cycles;

    public CpuEmulator6502(Memory<byte> memory)
    {
        state = new(memory);
        exec = new(state);
    }

    public void Reset()
    {
        state.Reg.PC = (ushort)(state.Mem[0xFFFC] | (state.Mem[0xFFFD] << 8));

        state.Reg.AC = 0;
        state.Reg.X = 0;
        state.Reg.Y = 0;

        state.Reg.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        state.Reg.SP = 0xFD;

        state.Cycles = 7;
    }

    public void Step()
    {
        var opcode = (CpuOpcode)state.Mem[state.Reg.PC++];
        if (!CpuOpcodeMap.TryDecodeOpcode(opcode, out var decode))
            throw new Exception(); // TODO

        var (instruction, mode) = decode;

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
}

