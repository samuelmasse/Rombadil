namespace Rombadil.Cpu.Emulator;

public class CpuEmulator6502(CpuEmulatorState state, CpuEmulatorBus bus)
{
    public void Reset(ushort? pc = null)
    {
        state.PC = pc ?? bus.Word(0xFFFC);

        state.AC = 0;
        state.X = 0;
        state.Y = 0;

        state.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        state.SP = 0xFD;

        state.Cycles = 7;
    }

    public void Step()
    {
        var code = bus[state.PC++];

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

    public void Nmi()
    {
        var s = state;
        var p = new CpuEmulatorProcessor(state, bus);
        var b = bus;

        p.PushWord(s.PC);
        p.Push((byte)(s.SR & ~CpuStatus.Break | CpuStatus.Unused));
        s.Interrupt = true;
        s.PC = b.Word(0xFFFA);
        s.Cycles += 7;
    }

    public void Irq()
    {
        if ((state.SR & CpuStatus.Interrupt) != 0)
            return;

        var p = new CpuEmulatorProcessor(state, bus);

        p.PushWord(state.PC);
        p.Push((byte)(state.SR & ~CpuStatus.Break | CpuStatus.Unused));
        state.Interrupt = true;
        state.PC = bus.Word(0xFFFE);
        state.Cycles += 7;
    }

    private void Step(CpuInstruction instruction, CpuAddressingMode mode)
    {
        var addr = Step(CpuEmulatorTimings.Get(instruction, mode), mode);
        var exec = new CpuEmulatorExecutor(state, bus, new(state, bus), new(state, bus, addr, mode));

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
        var addr = Step(CpuEmulatorIllegalTimings.Get(instruction, mode), mode);
        var exec = new CpuEmulatorIllegalExecutor(new(state, bus), new(state, bus, addr, mode));

        switch (instruction)
        {
            case CpuEmulatorIllegalInstruction.NOP: exec.Nop(); break;
            case CpuEmulatorIllegalInstruction.LAX: exec.Lax(); break;
            case CpuEmulatorIllegalInstruction.SAX: exec.Sax(); break;
            case CpuEmulatorIllegalInstruction.SBC: exec.Sbc(); break;
            case CpuEmulatorIllegalInstruction.DCP: exec.Dcp(); break;
            case CpuEmulatorIllegalInstruction.ISB: exec.Isb(); break;
            case CpuEmulatorIllegalInstruction.SLO: exec.Slo(); break;
            case CpuEmulatorIllegalInstruction.RLA: exec.Rla(); break;
            case CpuEmulatorIllegalInstruction.SRE: exec.Sre(); break;
            case CpuEmulatorIllegalInstruction.RRA: exec.Rra(); break;
        }
    }

    private ushort Step((byte, byte) timing, CpuAddressingMode mode)
    {
        var (addr, baseAddr) = Addr(state.PC, mode);
        var (cycles, pagePenalty) = timing;

        state.PC += (ushort)CpuAddressingModeSize.Get(mode);
        state.Cycles += cycles;
        if ((baseAddr & 0xFF00) != (addr & 0xFF00))
            state.Cycles += pagePenalty;

        return addr;
    }

    public (ushort, ushort) Addr(ushort pc, CpuAddressingMode mode) => mode switch
    {
        CpuAddressingMode.Immediate => (pc, pc),
        CpuAddressingMode.ZeroPage => (bus[pc], bus[pc]),
        CpuAddressingMode.ZeroPageX => ((byte)(bus[pc] + state.X), bus[pc]),
        CpuAddressingMode.ZeroPageY => ((byte)(bus[pc] + state.Y), bus[pc]),
        CpuAddressingMode.Absolute => (bus.Word(pc), bus.Word(pc)),
        CpuAddressingMode.AbsoluteX => ((ushort)(bus.Word(pc) + state.X), bus.Word(pc)),
        CpuAddressingMode.AbsoluteY => ((ushort)(bus.Word(pc) + state.Y), bus.Word(pc)),
        CpuAddressingMode.Indirect => (bus.WordPageWrap(bus.Word(pc)), bus.Word(pc)),
        CpuAddressingMode.IndirectX => (bus.WordZP((byte)(bus[pc] + state.X)), bus[pc]),
        CpuAddressingMode.IndirectY => ((ushort)(bus.WordZP(bus[pc]) + state.Y), bus.WordZP(bus[pc])),
        _ => (0, 0)
    };
}

