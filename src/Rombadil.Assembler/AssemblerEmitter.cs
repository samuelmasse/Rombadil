namespace Rombadil.Assembler;

internal class AssemblerEmitter(List<AssemblerStatement> statements, AssemblerResolver resolver, List<byte> output)
{
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

        if (!resolver.TryResolveEquation(instruction.Expression, out int arg))
            throw new Assembler6502Exception(statement.LineNumber,
                $"Unable to resolve operand value \"{instruction.Expression}\" for instruction \"{instruction.Instruction}\".");

        if (instruction.AddressingMode == CpuAddressingMode.Relative)
        {
            arg = arg - statement.MemoryLocation!.Value - 2;

            if (arg < -128 || arg > 127)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Relative branch offset {arg} is out of range for instruction \"{instruction.Instruction}\". " +
                    $"Expected signed 8-bit value (-128 to 127).");
        }

        output.Add((byte)opcode);

        var size = CpuAddressingModeSize.Get(instruction.AddressingMode);
        if (size == 1)
        {
            if (instruction.AddressingMode != CpuAddressingMode.Relative)
            {
                if (arg < 0 || arg > 0xFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Operand value {arg} is out of range for instruction \"{instruction.Instruction}\". " +
                        $"Expected 8-bit value (0 to 255).");
            }

            output.Add((byte)arg);
        }
        else if (size == 2)
        {
            if (arg < 0 || arg > 0xFFFF)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Operand value {arg} is out of range for instruction \"{instruction.Instruction}\". " +
                    $"Expected 16-bit value (0 to 65535).");

            output.Add((byte)arg);
            output.Add((byte)(arg >> 8));
        }
    }

    private void EmitDirective(AssemblerStatement statement, AssemblerDirective directive)
    {
        if (directive.Type == AssemblerDirectiveType.Byte)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Could not evaluate expression \"{expression}\" in \".byte\" directive.");

                if (val < 0 || val > 0xFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Value {val} is out of range for \".byte\" directive. Expected 8-bit unsigned value (0 to 255).");

                output.Add((byte)(val & 0xFF));
            }
        }
        else if (directive.Type == AssemblerDirectiveType.Word)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Could not evaluate expression \"{expression}\" in \".word\" directive.");

                if (val < 0 || val > 0xFFFF)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Value {val} is out of range for \".word\" directive. Expected 16-bit unsigned value (0 to 65535).");

                output.Add((byte)val);
                output.Add((byte)(val >> 8));
            }
        }
    }
}
