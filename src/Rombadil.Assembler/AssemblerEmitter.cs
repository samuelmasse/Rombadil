namespace Rombadil.Assembler;

internal class AssemblerEmitter(List<AssemblerStatement> statements, AssemblerResolver resolver, List<byte> output)
{
    internal void Emit()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];

            if (statement.Instruction != null)
                EmitInstruction(statement, statement.Instruction.Value);
            else if (statement.Directive != null)
                EmitDirective(statement, statement.Directive.Value);
        }
    }

    private void EmitInstruction(AssemblerStatement statement, AssemblerInstruction instruction)
    {
        if (!resolver.TryResolveEquation(instruction.Expression, out int arg))
            throw new Exception();

        if (instruction.AddressingMode == CpuAddressingMode.Relative)
            arg = arg - statement.MemoryLocation!.Value - 2;

        if (!CpuOpcodeMap.TryEncodeOpcode(instruction.Instruction, instruction.AddressingMode, out var opcode))
            throw new InvalidOperationException($"No opcode found for {instruction.Instruction} with {instruction.AddressingMode} addressing");

        output.Add((byte)opcode);

        var size = CpuAddressingModeSize.Get(instruction.AddressingMode);
        if (size >= 1)
            output.Add((byte)(arg & 0xFF));
        if (size >= 2)
            output.Add((byte)((arg >> 8) & 0xFF));
    }

    private void EmitDirective(AssemblerStatement statement, AssemblerDirective directive)
    {
        if (directive.Type == AssemblerDirectiveType.Byte)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Exception();

                output.Add((byte)(val & 0xFF));
            }
        }
        else if (directive.Type == AssemblerDirectiveType.Word)
        {
            foreach (var expression in directive.Expressions)
            {
                if (!resolver.TryResolveEquation(expression, out int val))
                    throw new Exception();

                output.Add((byte)(val & 0xFF));
                output.Add((byte)((val >> 8) & 0xFF));
            }
        }
    }
}
