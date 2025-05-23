namespace Rombadil.Assembler;

public class CompilationStage2(
    CompilationStatements statements,
    CompilationConstants constants,
    CompilationResolver resolver,
    CompilationAdressingModeResolver adressingModeResolver,
    CompilationInstructionStatements instructionStatements,
    CompilationDirectiveStatements directiveStatements,
    CompilationMemoryLayout memoryLayout)
{
    public byte[] Compile()
    {
        PopulateConstantLocations();
        ParseOperationStatements();
        CreateMemoryLayout();
        ResolveLabelValues();
        return EmitBinary();
    }

    private void PopulateConstantLocations()
    {
        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var line = statements.Statements[i];
            if (line.Type == StatementType.Constant || line.Type == StatementType.Label)
                constants.Declare(line.Name, i);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var statement = statements.Statements[i];
            if (statement.Type == StatementType.Operation)
                ParseOperationStatement(statement, i);
        }
    }

    private void ParseOperationStatement(Statement statement, int i)
    {
        if (statement.Name.StartsWith('.'))
            ParseDirectiveStatement(statement, i);
        else ParseInstructionStatement(statement, i);
    }

    private void ParseDirectiveStatement(Statement statement, int i)
    {
        var noDot = statement.Name[1..];

        if (!Enum.TryParse<DirectiveType>(noDot, true, out var directiveType))
            throw new Exception(); // TODO

        var expressions = statement.Value.Split(',');
        directiveStatements[i] = new(directiveType, expressions);
    }

    private void ParseInstructionStatement(Statement statement, int i)
    {
        if (!Enum.TryParse<CpuInstruction>(statement.Name, true, out var instruction))
            throw new Exception(); // TODO

        var operand = statement.Value;
        var (adressingMode, expression) = adressingModeResolver.Resolve(instruction, operand);
        instructionStatements[i] = new(instruction, adressingMode, expression);
    }

    private void CreateMemoryLayout()
    {
        int index = 0;

        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var instructionStatement = instructionStatements[i];
            if (instructionStatement != null)
            {
                memoryLayout[i] = index;
                index += 1 + OperandSize(instructionStatement.Value.AdressingMode);
                continue;
            }

            var directiveStatement = directiveStatements[i];
            if (directiveStatement != null)
            {
                var dirst = directiveStatement.Value;

                if (dirst.Type == DirectiveType.Word)
                {
                    memoryLayout[i] = index;
                    index += 2 * dirst.Expressions.Length;
                    continue;
                }

                if (dirst.Type == DirectiveType.Byte)
                {
                    memoryLayout[i] = index;
                    index += dirst.Expressions.Length;
                    continue;
                }

                if (dirst.Type == DirectiveType.Org)
                {
                    if (!resolver.TryResolveEquation(dirst.Expressions[0], out var jmp))
                        throw new Exception(); // TODO

                    index = jmp;
                    continue;
                }
            }
        }
    }

    private void ResolveLabelValues()
    {
        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var statement = statements.Statements[i];
            if (statement.Type == StatementType.Label)
                ResolveLabelValue(statement.Name, i);
        }
    }

    private void ResolveLabelValue(string name, int i)
    {
        int nextIndex = i;
        while (nextIndex < statements.Statements.Length)
        {
            int? memoryValue = memoryLayout[nextIndex];
            if (memoryValue != null)
            {
                constants.SetValue(name, memoryValue.Value);
                return;
            }

            nextIndex++;
        }

        throw new Exception();
    }

    private byte[] EmitBinary()
    {
        var output = new List<byte>();

        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var instructionStatement = instructionStatements[i];
            if (instructionStatement != null)
            {
                var istat = instructionStatement.Value;

                if (!resolver.TryResolveEquation(istat.Expression, out int arg))
                    throw new Exception();

                if (istat.AdressingMode == CpuAdressingMode.Relative)
                    arg = arg - memoryLayout[i]!.Value - 2;

                if (!CpuOpcodeMap.TryEncodeOpcode(istat.Instruction, istat.AdressingMode, out var opcode))
                    throw new InvalidOperationException($"No opcode found for {istat.Instruction} with {istat.AdressingMode} addressing");
                output.Add((byte)opcode);

                var size = OperandSize(istat.AdressingMode);
                if (size >= 1)
                    output.Add((byte)(arg & 0xFF));
                if (size >= 2)
                    output.Add((byte)((arg >> 8) & 0xFF));

                continue;
            }

            var directiveStatement = directiveStatements[i];
            if (directiveStatement != null)
            {
                var dstat = directiveStatement.Value;

                if (dstat.Type == DirectiveType.Byte)
                {
                    foreach (var expression in dstat.Expressions)
                    {
                        if (!resolver.TryResolveEquation(expression, out int val))
                            throw new Exception();

                        output.Add((byte)(val & 0xFF));
                    }
                }
                else if (dstat.Type == DirectiveType.Word)
                {
                    foreach (var expression in dstat.Expressions)
                    {
                        if (!resolver.TryResolveEquation(expression, out int val))
                            throw new Exception();

                        output.Add((byte)(val & 0xFF));
                        output.Add((byte)((val >> 8) & 0xFF));
                    }
                }
            }
        }

        return [.. output];
    }

    private static int OperandSize(CpuAdressingMode mode)
    {
        if (mode == CpuAdressingMode.Indirect ||
            mode == CpuAdressingMode.Absolute ||
            mode == CpuAdressingMode.AbsoluteX ||
            mode == CpuAdressingMode.AbsoluteY)
            return 2;
        else if (mode == CpuAdressingMode.Implied)
            return 0;
        else return 1;
    }
}
