namespace Rombadil.Assembler;

internal class AssemblerEmitter(
    IReadOnlyList<AssemblerSegment> segments,
    List<AssemblerStatement> statements,
    AssemblerResolver resolver,
    List<byte> output)
{
    private readonly List<bool> written = [];
    private int cursor;

    internal void Emit()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];

            if (statement.Instruction != null)
                EmitInstruction(statement, statement.Instruction);
            else if (statement.Directive != null)
                EmitDirective(statement, statement.Directive);
        }
    }

    private void EmitInstruction(AssemblerStatement statement, AssemblerInstruction instruction)
    {
        if (!CpuOpcodeMap.TryEncodeOpcode(instruction.Instruction, instruction.AddressingMode, out var opcode))
            throw new Assembler6502Exception(statement.LineNumber,
                $"No opcode exists for instruction \"{instruction.Instruction}\" with addressing mode \"{instruction.AddressingMode}\".");

        if (!resolver.TryResolveEquation(instruction.Expression, out var arg))
            throw new Assembler6502Exception(statement.LineNumber,
                $"Unable to resolve operand value \"{instruction.Expression}\" for instruction \"{instruction.Instruction}\".");

        if (instruction.AddressingMode == CpuAddressingMode.Relative)
        {
            if (arg.IncludesLabel)
                arg.Value = arg.Value - statement.MemoryLocation!.Value - 2;

            if (arg.Value < -128 || arg.Value > 127)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Relative branch offset {arg.Value} is out of range for instruction \"{instruction.Instruction}\". " +
                    $"Expected signed 8-bit value (-128 to 127).");
        }

        Write(statement, (byte)opcode);

        var size = CpuAddressingModeSize.Get(instruction.AddressingMode);
        if (size == 1)
        {
            if (instruction.AddressingMode != CpuAddressingMode.Relative)
            {
                if (arg.Value < 0 || arg.Value > 0xFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Operand value {arg.Value} is out of range for instruction \"{instruction.Instruction}\". " +
                        $"Expected 8-bit value (0 to 255).");
            }

            Write(statement, (byte)arg.Value);
        }
        else if (size == 2)
        {
            if (arg.Value < 0 || arg.Value > 0xFFFF)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Operand value {arg.Value} is out of range for instruction \"{instruction.Instruction}\". " +
                    $"Expected 16-bit value (0 to 65535).");

            Write(statement, (byte)arg.Value);
            Write(statement, (byte)(arg.Value >> 8));
        }
    }

    private void EmitDirective(AssemblerStatement statement, AssemblerDirective directive)
    {
        if (directive.Type == AssemblerDirectiveType.Byte)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out var val))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Could not evaluate expression \"{expression}\" in \".byte\" directive.");

                if (val.Value < 0 || val.Value > 0xFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Value {val.Value} is out of range for \".byte\" directive. Expected 8-bit unsigned value (0 to 255).");

                Write(statement, (byte)(val.Value & 0xFF));
            }
        }
        else if (directive.Type == AssemblerDirectiveType.Word)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out var val))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Could not evaluate expression \"{expression}\" in \".word\" directive.");

                if (val.Value < 0 || val.Value > 0xFFFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Value {val.Value} is out of range for \".word\" directive. Expected 16-bit unsigned value (0 to 65535).");

                Write(statement, (byte)val.Value);
                Write(statement, (byte)(val.Value >> 8));
            }
        }
        else if (directive.Type == AssemblerDirectiveType.Incbin)
        {
            var bytes = statement.IncludedBytes!;
            foreach (var b in bytes)
                Write(statement, b);
        }
        else if (directive.Type == AssemblerDirectiveType.Segment)
        {
            cursor = 0;

            foreach (var segment in segments)
            {
                if (segment == statement.Segment)
                    break;

                cursor += segment.FileSize;
            }

            while (output.Count - cursor < statement.Segment!.FileSize)
            {
                output.Add(0);
                written.Add(false);
            }
        }
    }

    private void Write(AssemblerStatement statement, byte b)
    {
        while (output.Count <= cursor)
        {
            output.Add(0);
            written.Add(false);
        }

        if (written[cursor])
            throw new Assembler6502Exception(statement.LineNumber,
                $"Multiple writes to file offset {cursor:X4}. " +
                $"This indicates overlapping output, possibly due to conflicting segment layout or repeated memory assignments.");

        output[cursor] = b;
        written[cursor] = true;

        cursor++;
    }
}
