namespace Rombadil.Cpu.Emulator;

public class CpuEmulator6502
{
    private readonly CpuEmulatorMemory memory;
    private readonly CpuEmulatorState state;
    private readonly CpuEmulatorProcessor processor;
    private readonly CpuEmulatorHelper helper;
    private readonly CpuEmulatorTimings timings;
    private readonly CpuEmulatorIllegalTimings illegalTimings;

    public CpuEmulatorRegisters Reg => state.Reg;
    public long Cycles => state.Cycles;
    internal CpuEmulatorHelper Helper => helper;

    public CpuEmulator6502(CpuEmulatorMemory memory)
    {
        this.memory = memory;
        state = new();
        processor = new(state);
        helper = new(state, memory);
        timings = new();
        illegalTimings = new();
    }

    public void Reset(ushort? pc = null)
    {
        state.PC = pc ?? (ushort)(memory[0xFFFC] | (memory[0xFFFD] << 8));

        state.AC = 0;
        state.X = 0;
        state.Y = 0;

        state.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        state.SP = 0xFD;

        state.Cycles = 7;
    }

    public void Step()
    {
        var code = memory[state.PC++];

        if (CpuOpcodeMap.TryDecodeOpcode((CpuOpcode)code, out var decode))
        {
            var (instruction, mode) = decode;
            Step(instruction, mode);
        }
        else if (CpuEmulatorIllegalOpcodeMap.TryDecodeOpcode((CpuEmulatorIllegalOpcode)code, out var illegal))
        {
            var (instruction, mode) = illegal;
            Step(instruction, mode);
        }
        else throw new Exception();
    }

    private void Step(CpuInstruction instruction, CpuAddressingMode mode)
    {
        var addr = Step(timings[instruction, mode], mode);
        var exec = new CpuEmulatorExecutor(memory, helper, state, processor, addr,
            ref mode == CpuAddressingMode.Accumulator ? ref state.AC : ref memory[addr]);

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

    private void Step(CpuEmulatorIllegalInstruction instruction, CpuAddressingMode mode)
    {
        var addr = Step(illegalTimings[instruction, mode], mode);
        var ixec = new CpuEmulatorIllegalExecutor(memory, helper, state, processor, addr, ref memory[addr]);

        switch (instruction)
        {
            case CpuEmulatorIllegalInstruction.NOP: ixec.Nop(); break;
            case CpuEmulatorIllegalInstruction.LAX: ixec.Lax(); break;
            case CpuEmulatorIllegalInstruction.SAX: ixec.Sax(); break;
            case CpuEmulatorIllegalInstruction.SBC: ixec.Sbc(); break;
            case CpuEmulatorIllegalInstruction.DCP: ixec.Dcp(); break;
            case CpuEmulatorIllegalInstruction.ISB: ixec.Isb(); break;
            case CpuEmulatorIllegalInstruction.SLO: ixec.Slo(); break;
            case CpuEmulatorIllegalInstruction.RLA: ixec.Rla(); break;
            case CpuEmulatorIllegalInstruction.SRE: ixec.Sre(); break;
            case CpuEmulatorIllegalInstruction.RRA: ixec.Rra(); break;
        }
    }

    internal ushort Step((byte, byte) timing, CpuAddressingMode mode)
    {
        var (addr, baseAddr) = helper.Resolve(state.PC, mode);
        var (cycles, pagePenalty) = timing;

        state.PC += (ushort)CpuAddressingModeSize.Get(mode);
        state.Cycles += cycles;
        if ((baseAddr & 0xFF00) != (addr & 0xFF00))
            state.Cycles += pagePenalty;

        return addr;
    }
}

