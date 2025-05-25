namespace Rombadil.Cpu.Emulator;

public class Cpu6502(Memory<byte> memory)
{
    private CpuRegisters reg;
    private long cycles;

    public CpuRegisters Reg => reg;
    public long Cycles => cycles;

    public void Reset()
    {
        reg.PC = (ushort)(memory.Span[0xFFFC] | (memory.Span[0xFFFD] << 8));

        reg.AC = 0;
        reg.X = 0;
        reg.Y = 0;

        reg.SR = CpuStatus.Interrupt | CpuStatus.Unused;
        reg.SP = 0xFD;

        cycles = 7;
    }

    public void Step()
    {
        var opcode = (CpuOpcode)memory.Span[reg.PC++];
        Execute(opcode);
    }

    private void Execute(CpuOpcode opcode)
    {
        if (!CpuOpcodeMap.TryDecodeOpcode(opcode, out var decode))
            throw new Exception(); // TODO

        var (instruction, mode) = decode;

        switch (instruction)
        {
            case CpuInstruction.ADC:
                ExecuteAdc(mode);
                break;
            case CpuInstruction.AND:
                ExecuteAnd(mode);
                break;
            case CpuInstruction.ASL:
                ExecuteAsl(mode);
                break;
            case CpuInstruction.BIT:
                ExecuteBit(mode);
                break;
            case CpuInstruction.BPL:
                ExecuteBpl();
                break;
            case CpuInstruction.BMI:
                ExecuteBmi();
                break;
            case CpuInstruction.BVC:
                ExecuteBvc();
                break;
            case CpuInstruction.BVS:
                ExecuteBvs();
                break;
            case CpuInstruction.BCC:
                ExecuteBcc();
                break;
            case CpuInstruction.BCS:
                ExecuteBcs();
                break;
            case CpuInstruction.BNE:
                ExecuteBne();
                break;
            case CpuInstruction.BEQ:
                ExecuteBeq();
                break;
            case CpuInstruction.BRK:
                ExecuteBrk();
                break;
            case CpuInstruction.CMP:
                ExecuteCmp(mode);
                break;
            case CpuInstruction.CPX:
                ExecuteCpx(mode);
                break;
            case CpuInstruction.CPY:
                ExecuteCpy(mode);
                break;
            case CpuInstruction.DEC:
                ExecuteDec(mode);
                break;
            case CpuInstruction.EOR:
                ExecuteEor(mode);
                break;
            case CpuInstruction.CLC:
                ExecuteClc();
                break;
            case CpuInstruction.SEC:
                ExecuteSec();
                break;
            case CpuInstruction.CLI:
                ExecuteCli();
                break;
            case CpuInstruction.SEI:
                ExecuteSei();
                break;
            case CpuInstruction.CLV:
                ExecuteClv();
                break;
            case CpuInstruction.CLD:
                ExecuteCld();
                break;
            case CpuInstruction.SED:
                ExecuteSed();
                break;
            case CpuInstruction.INC:
                ExecuteInc(mode);
                break;
            case CpuInstruction.JMP:
                ExecuteJmp(mode);
                break;
            case CpuInstruction.JSR:
                ExecuteJsr();
                break;
            case CpuInstruction.LDA:
                ExecuteLda(mode);
                break;
            case CpuInstruction.LDX:
                ExecuteLdx(mode);
                break;
            case CpuInstruction.LDY:
                ExecuteLdy(mode);
                break;
            case CpuInstruction.LSR:
                ExecuteLsr(mode);
                break;
            case CpuInstruction.NOP:
                ExecuteNop();
                break;
            case CpuInstruction.ORA:
                ExecuteOra(mode);
                break;
            case CpuInstruction.TAX:
                ExecuteTax();
                break;
            case CpuInstruction.TXA:
                ExecuteTxa();
                break;
            case CpuInstruction.DEX:
                ExecuteDex();
                break;
            case CpuInstruction.INX:
                ExecuteInx();
                break;
            case CpuInstruction.TAY:
                ExecuteTay();
                break;
            case CpuInstruction.TYA:
                ExecuteTya();
                break;
            case CpuInstruction.DEY:
                ExecuteDey();
                break;
            case CpuInstruction.INY:
                ExecuteIny();
                break;
            case CpuInstruction.ROL:
                ExecuteRol(mode);
                break;
            case CpuInstruction.ROR:
                ExecuteRor(mode);
                break;
            case CpuInstruction.RTI:
                ExecuteRti();
                break;
            case CpuInstruction.RTS:
                ExecuteRts();
                break;
            case CpuInstruction.SBC:
                ExecuteSbc(mode);
                break;
            case CpuInstruction.STA:
                ExecuteSta(mode);
                break;
            case CpuInstruction.TXS:
                ExecuteTxs();
                break;
            case CpuInstruction.TSX:
                ExecuteTsx();
                break;
            case CpuInstruction.PHA:
                ExecutePha();
                break;
            case CpuInstruction.PLA:
                ExecutePla();
                break;
            case CpuInstruction.PHP:
                ExecutePhp();
                break;
            case CpuInstruction.PLP:
                ExecutePlp();
                break;
            case CpuInstruction.STX:
                ExecuteStx(mode);
                break;
            case CpuInstruction.STY:
                ExecuteSty(mode);
                break;
            default:
                break;
        }
    }

    private void ExecuteAdc(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.ADC, mode);
        byte value = memory.Span[addr];

        int carry = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int sum = reg.AC + value + carry;

        SetFlag(CpuStatus.Carry, sum > 0xFF);

        byte result = (byte)sum;

        bool overflow = ((reg.AC ^ result) & (value ^ result) & 0x80) != 0;
        SetFlag(CpuStatus.Overflow, overflow);

        reg.AC = result;
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteAnd(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.AND, mode);
        reg.AC &= memory.Span[addr];
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteAsl(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            SetFlag(CpuStatus.Carry, (reg.AC & 0x80) != 0);
            reg.AC <<= 1;
            UpdateZeroNegativeFlags(reg.AC);
            cycles += CpuOpcodeTimings.Get(CpuInstruction.ASL, mode).Cycles;
            return;
        }

        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.ASL, mode);
        ref byte value = ref memory.Span[addr];

        SetFlag(CpuStatus.Carry, (value & 0x80) != 0);
        value <<= 1;
        UpdateZeroNegativeFlags(value);
    }

    private void ExecuteBit(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.BIT, mode);
        byte value = memory.Span[addr];

        SetFlag(CpuStatus.Zero, (reg.AC & value) == 0);
        SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
        SetFlag(CpuStatus.Overflow, (value & 0b0100_0000) != 0);
    }

    private void UpdateZeroNegativeFlags(byte value)
    {
        SetFlag(CpuStatus.Zero, value == 0);
        SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
    }

    private void ExecuteBpl() => ExecuteBranch(CpuInstruction.BPL, (reg.SR & CpuStatus.Negative) == 0);
    private void ExecuteBmi() => ExecuteBranch(CpuInstruction.BMI, (reg.SR & CpuStatus.Negative) != 0);

    private void ExecuteBvc() => ExecuteBranch(CpuInstruction.BVC, (reg.SR & CpuStatus.Overflow) == 0);
    private void ExecuteBvs() => ExecuteBranch(CpuInstruction.BVS, (reg.SR & CpuStatus.Overflow) != 0);

    private void ExecuteBcc() => ExecuteBranch(CpuInstruction.BCC, (reg.SR & CpuStatus.Carry) == 0);
    private void ExecuteBcs() => ExecuteBranch(CpuInstruction.BCS, (reg.SR & CpuStatus.Carry) != 0);

    private void ExecuteBne() => ExecuteBranch(CpuInstruction.BNE, (reg.SR & CpuStatus.Zero) == 0);
    private void ExecuteBeq() => ExecuteBranch(CpuInstruction.BEQ, (reg.SR & CpuStatus.Zero) != 0);

    private void ExecuteBrk()
    {
        reg.PC++; // Skip padding byte after $00 opcode (even if unused)

        // Push PC high then low (return address = PC + 1)
        Push((byte)((reg.PC >> 8) & 0xFF));
        Push((byte)(reg.PC & 0xFF));

        // Push status with Break flag set (bit 4 = 1), bit 5 always set
        byte flags = (byte)(reg.SR | CpuStatus.Break | CpuStatus.Unused);
        Push(flags);

        // Set Interrupt disable
        SetFlag(CpuStatus.Interrupt, true);

        // Jump to vector at $FFFE-$FFFF
        ushort vector = (ushort)(memory.Span[0xFFFE] | (memory.Span[0xFFFF] << 8));
        reg.PC = vector;

        cycles += 7;
    }

    private void ExecuteCmp(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.CMP, mode);
        byte value = memory.Span[addr];

        int result = reg.AC - value;

        SetFlag(CpuStatus.Carry, reg.AC >= value);
        SetFlag(CpuStatus.Zero, reg.AC == value);
        SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    private void ExecuteCpx(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.CPX, mode);
        byte value = memory.Span[addr];

        int result = reg.X - value;

        SetFlag(CpuStatus.Carry, reg.X >= value);
        SetFlag(CpuStatus.Zero, reg.X == value);
        SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    private void ExecuteCpy(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.CPY, mode);
        byte value = memory.Span[addr];

        int result = reg.Y - value;

        SetFlag(CpuStatus.Carry, reg.Y >= value);
        SetFlag(CpuStatus.Zero, reg.Y == value);
        SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    private void ExecuteDec(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.DEC, mode);
        ref byte value = ref memory.Span[addr];

        value--;
        UpdateZeroNegativeFlags(value);
    }

    private void ExecuteEor(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.EOR, mode);
        reg.AC ^= memory.Span[addr];
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteClc()
    {
        SetFlag(CpuStatus.Carry, false);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.CLC, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteSec()
    {
        SetFlag(CpuStatus.Carry, true);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.SEC, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteCli()
    {
        SetFlag(CpuStatus.Interrupt, false);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.CLI, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteSei()
    {
        SetFlag(CpuStatus.Interrupt, true);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.SEI, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteClv()
    {
        SetFlag(CpuStatus.Overflow, false);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.CLV, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteCld()
    {
        SetFlag(CpuStatus.Decimal, false);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.CLD, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteSed()
    {
        SetFlag(CpuStatus.Decimal, true);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.SED, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteInc(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.INC, mode);
        ref byte value = ref memory.Span[addr];

        value++;
        UpdateZeroNegativeFlags(value);
    }

    private void ExecuteJmp(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Absolute)
        {
            reg.PC = ReadWord();
        }
        else if (mode == CpuAddressingMode.Indirect)
        {
            ushort addr = ReadWord();

            // Emulate JMP ($xxFF) bug — no page carry
            var span = memory.Span;
            byte low = span[addr];
            byte high = span[(ushort)((addr & 0xFF00) | ((addr + 1) & 0x00FF))];

            reg.PC = (ushort)(low | (high << 8));
        }

        cycles += CpuOpcodeTimings.Get(CpuInstruction.JMP, mode).Cycles;
    }

    private void ExecuteJsr()
    {
        ushort target = ReadWord();

        ushort returnAddr = (ushort)(reg.PC - 1);
        Push((byte)((returnAddr >> 8) & 0xFF));
        Push((byte)(returnAddr & 0xFF));

        reg.PC = target;
        cycles += CpuOpcodeTimings.Get(CpuInstruction.JSR, CpuAddressingMode.Absolute).Cycles;
    }

    private void ExecuteLda(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.LDA, mode);
        reg.AC = memory.Span[addr];
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteLdx(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.LDX, mode);
        reg.X = memory.Span[addr];
        UpdateZeroNegativeFlags(reg.X);
    }

    private void ExecuteLdy(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.LDY, mode);
        reg.Y = memory.Span[addr];
        UpdateZeroNegativeFlags(reg.Y);
    }

    private void ExecuteLsr(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            SetFlag(CpuStatus.Carry, (reg.AC & 0x01) != 0);
            reg.AC >>= 1;
            SetFlag(CpuStatus.Negative, false);
            SetFlag(CpuStatus.Zero, reg.AC == 0);

            cycles += CpuOpcodeTimings.Get(CpuInstruction.LSR, mode).Cycles;
            return;
        }

        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.LSR, mode);
        ref byte value = ref memory.Span[addr];

        SetFlag(CpuStatus.Carry, (value & 0x01) != 0);
        value >>= 1;
        SetFlag(CpuStatus.Negative, false);
        SetFlag(CpuStatus.Zero, value == 0);
    }

    private void ExecuteNop()
    {
        cycles += CpuOpcodeTimings.Get(CpuInstruction.NOP, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteOra(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.ORA, mode);
        reg.AC |= memory.Span[addr];
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteTax()
    {
        reg.X = reg.AC;
        UpdateZeroNegativeFlags(reg.X);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TAX, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteTxa()
    {
        reg.AC = reg.X;
        UpdateZeroNegativeFlags(reg.AC);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TXA, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteTay()
    {
        reg.Y = reg.AC;
        UpdateZeroNegativeFlags(reg.Y);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TAY, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteTya()
    {
        reg.AC = reg.Y;
        UpdateZeroNegativeFlags(reg.AC);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TYA, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteDex()
    {
        reg.X--;
        UpdateZeroNegativeFlags(reg.X);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.DEX, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteInx()
    {
        reg.X++;
        UpdateZeroNegativeFlags(reg.X);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.INX, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteDey()
    {
        reg.Y--;
        UpdateZeroNegativeFlags(reg.Y);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.DEY, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteIny()
    {
        reg.Y++;
        UpdateZeroNegativeFlags(reg.Y);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.INY, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteRol(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            int carryIn = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
            bool carryOut = (reg.AC & 0x80) != 0;

            reg.AC = (byte)((reg.AC << 1) | carryIn);

            SetFlag(CpuStatus.Carry, carryOut);
            UpdateZeroNegativeFlags(reg.AC);

            cycles += CpuOpcodeTimings.Get(CpuInstruction.ROL, mode).Cycles;
            return;
        }

        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.ROL, mode);
        ref byte value = ref memory.Span[addr];

        int carryInMem = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOutMem = (value & 0x80) != 0;

        value = (byte)((value << 1) | carryInMem);

        SetFlag(CpuStatus.Carry, carryOutMem);
        UpdateZeroNegativeFlags(value);
    }

    private void ExecuteRor(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            int carryIn = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
            bool carryOut = (reg.AC & 0x01) != 0;

            reg.AC = (byte)((reg.AC >> 1) | (carryIn << 7));

            SetFlag(CpuStatus.Carry, carryOut);
            UpdateZeroNegativeFlags(reg.AC);

            cycles += CpuOpcodeTimings.Get(CpuInstruction.ROR, mode).Cycles;
            return;
        }

        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.ROR, mode);
        ref byte value = ref memory.Span[addr];

        int carryInMem = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOutMem = (value & 0x01) != 0;

        value = (byte)((value >> 1) | (carryInMem << 7));

        SetFlag(CpuStatus.Carry, carryOutMem);
        UpdateZeroNegativeFlags(value);
    }

    private void ExecuteBranch(CpuInstruction instr, bool condition)
    {
        var timing = CpuOpcodeTimings.Get(instr, CpuAddressingMode.Relative);
        cycles += timing.Cycles;

        sbyte offset = (sbyte)memory.Span[reg.PC++];
        if (!condition)
            return;

        cycles += 1;

        ushort originalPC = reg.PC;
        reg.PC = (ushort)(reg.PC + offset);

        if ((originalPC & 0xFF00) != (reg.PC & 0xFF00))
            cycles += timing.PagePenalty;
    }

    private void ExecuteRti()
    {
        // Pull status first (flags)
        byte flags = Pop();
        reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused); // B is cleared, bit 5 stays set

        // Pull PC low, then high
        byte pcl = Pop();
        byte pch = Pop();
        reg.PC = (ushort)(pcl | (pch << 8));

        cycles += CpuOpcodeTimings.Get(CpuInstruction.RTI, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteRts()
    {
        // Pull PC low, then high
        byte pcl = Pop();
        byte pch = Pop();
        reg.PC = (ushort)((pcl | (pch << 8)) + 1);

        cycles += CpuOpcodeTimings.Get(CpuInstruction.RTS, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteSbc(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.SBC, mode);
        byte value = memory.Span[addr];

        int carry = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int result = reg.AC - value - (1 - carry);

        // Set Carry: set if no borrow occurred (i.e., A ≥ value + (1 - C))
        SetFlag(CpuStatus.Carry, result >= 0);

        byte resultByte = (byte)result;

        // Set Overflow: if sign bit flipped incorrectly during subtraction
        bool overflow = ((reg.AC ^ value) & (reg.AC ^ resultByte) & 0x80) != 0;
        SetFlag(CpuStatus.Overflow, overflow);

        reg.AC = resultByte;
        UpdateZeroNegativeFlags(reg.AC);
    }

    private void ExecuteSta(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.STA, mode);
        memory.Span[addr] = reg.AC;
    }

    private void ExecuteTxs()
    {
        reg.SP = reg.X;
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TXS, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteTsx()
    {
        reg.X = reg.SP;
        UpdateZeroNegativeFlags(reg.X);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.TSX, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecutePha()
    {
        Push(reg.AC);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.PHA, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecutePla()
    {
        reg.AC = Pop();
        UpdateZeroNegativeFlags(reg.AC);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.PLA, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecutePhp()
    {
        // Push status with Break and Unused bits set (like BRK)
        byte flags = (byte)(reg.SR | CpuStatus.Break | CpuStatus.Unused);
        Push(flags);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.PHP, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecutePlp()
    {
        byte flags = Pop();
        reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        cycles += CpuOpcodeTimings.Get(CpuInstruction.PLP, CpuAddressingMode.Implied).Cycles;
    }

    private void ExecuteStx(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.STX, mode);
        memory.Span[addr] = reg.X;
    }

    private void ExecuteSty(CpuAddressingMode mode)
    {
        ushort addr = ResolveOperandAddressWithCycles(CpuInstruction.STY, mode);
        memory.Span[addr] = reg.Y;
    }

    private void SetFlag(CpuStatus flag, bool on)
    {
        if (on) reg.SR |= flag;
        else reg.SR &= ~flag;
    }

    private void Push(byte value)
    {
        memory.Span[0x0100 + reg.SP--] = value;
    }

    private byte Pop()
    {
        return memory.Span[0x0100 + ++reg.SP];
    }

    private ushort ResolveOperandAddressWithCycles(CpuInstruction instr, CpuAddressingMode mode)
    {
        var mem = memory.Span;

        ushort? baseAddr = null;
        ushort? effectiveAddr = null;
        ushort resolved = 0;

        switch (mode)
        {
            case CpuAddressingMode.Immediate:
                resolved = reg.PC++;
                break;

            case CpuAddressingMode.ZeroPage:
                resolved = mem[reg.PC++];
                break;

            case CpuAddressingMode.ZeroPageX:
                resolved = (byte)(mem[reg.PC++] + reg.X);
                break;

            case CpuAddressingMode.ZeroPageY:
                resolved = (byte)(mem[reg.PC++] + reg.Y);
                break;

            case CpuAddressingMode.Absolute:
                resolved = ReadWord();
                break;

            case CpuAddressingMode.AbsoluteX:
                baseAddr = ReadWord();
                effectiveAddr = (ushort)(baseAddr.Value + reg.X);
                resolved = effectiveAddr.Value;
                break;

            case CpuAddressingMode.AbsoluteY:
                baseAddr = ReadWord();
                effectiveAddr = (ushort)(baseAddr.Value + reg.Y);
                resolved = effectiveAddr.Value;
                break;

            case CpuAddressingMode.IndirectX:
                byte zpx = (byte)(mem[reg.PC++] + reg.X);
                resolved = (ushort)(mem[zpx] | (mem[(byte)(zpx + 1)] << 8));
                break;

            case CpuAddressingMode.IndirectY:
                byte zpy = mem[reg.PC++];
                baseAddr = (ushort)(mem[zpy] | (mem[(byte)(zpy + 1)] << 8));
                effectiveAddr = (ushort)(baseAddr.Value + reg.Y);
                resolved = effectiveAddr.Value;
                break;
        }

        var timing = CpuOpcodeTimings.Get(instr, mode);

        cycles += timing.Cycles;
        if (baseAddr.HasValue && effectiveAddr.HasValue && (baseAddr.Value & 0xFF00) != (effectiveAddr.Value & 0xFF00))
            cycles += timing.PagePenalty;

        return resolved;
    }

    private ushort ReadWord()
    {
        ushort value = (ushort)(memory.Span[reg.PC] | (memory.Span[reg.PC + 1] << 8));
        reg.PC += 2;
        return value;
    }
}

