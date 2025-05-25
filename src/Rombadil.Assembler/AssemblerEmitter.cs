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
                $"No opcode found for \"{instruction.Instruction}\" with \"{instruction.AddressingMode}\" addressing.");

        if (!resolver.TryResolveEquation(instruction.Expression, out int arg))
            throw new Assembler6502Exception(statement.LineNumber,
                $"Unable to resolve operand value \"{instruction.Expression}\" of \"{instruction.Instruction}\".");

        if (instruction.AddressingMode == CpuAddressingMode.Relative)
        {
            arg = arg - statement.MemoryLocation!.Value - 2;

            if (arg < -128 || arg > 127)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Operand is outside of valid relative range value \"{arg}\" for {instruction.Instruction}");
        }

        output.Add((byte)opcode);

        var size = CpuAddressingModeSize.Get(instruction.AddressingMode);
        if (size == 1)
        {
            if (arg > 0xFF)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Operand is outside of valid single byte range value \"{arg}\" for {instruction.Instruction}");

            output.Add((byte)arg);
        }
        else if (size == 2)
        {
            if (arg > 0xFFFF)
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Operand is outside of valid two byte range value \"{arg}\" for {instruction.Instruction}");

            output.Add((byte)(arg & 0xFF));
            output.Add((byte)((arg >> 8) & 0xFF));
        }
    }

    private void EmitDirective(AssemblerStatement statement, AssemblerDirective directive)
    {
        if (directive.Type == AssemblerDirectiveType.Byte)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Assembler6502Exception(statement.LineNumber, $"Unable to resolve .byte value \"{expression}\".");

                output.Add((byte)(val & 0xFF));
            }
        }
        else if (directive.Type == AssemblerDirectiveType.Word)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Assembler6502Exception(statement.LineNumber, $"Unable to resolve .word value \"{expression}\".");

                output.Add((byte)(val & 0xFF));
                output.Add((byte)((val >> 8) & 0xFF));
            }
        }
    }
}
