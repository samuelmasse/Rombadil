namespace Rombadil.Assembler;

public class CompilationStage(
    string[] lines,
    StatementParser statementParser,
    List<Statement> statements,
    CompilationConstants constants,
    CompilationResolver resolver,
    CompilationAdressingModeResolver adressingModeResolver)
{
    public byte[] Compile()
    {
        foreach (var statement in statementParser.Parse(lines))
            statements.Add(statement);

        PopulateConstantLocations();
        ParseOperationStatements();
        CreateMemoryLayout();
        ResolveLabelValues();
        return EmitBinary();
    }


    private void PopulateConstantLocations()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var line = statements[i];
            if (line.Type == StatementType.Constant || line.Type == StatementType.Label)
                constants.Declare(line.Name, i);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
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
        statements[i].DirectiveStatement = new(directiveType, expressions);
    }

    private void ParseInstructionStatement(Statement statement, int i)
    {
        if (!Enum.TryParse<CpuInstruction>(statement.Name, true, out var instruction))
            throw new Exception(); // TODO

        var operand = statement.Value;
        var (adressingMode, expression) = adressingModeResolver.Resolve(instruction, operand);
        statements[i].InstructionStatement = new(instruction, adressingMode, expression);
    }

    private void CreateMemoryLayout()
    {
        int index = 0;

        for (int i = 0; i < statements.Count; i++)
        {
            var instructionStatement = statements[i].InstructionStatement;
            if (instructionStatement != null)
            {
                statements[i].MemoryLocation = index;
                index += 1 + OperandSize(instructionStatement.Value.AdressingMode);
                continue;
            }

            var directiveStatement = statements[i].DirectiveStatement;
            if (directiveStatement != null)
            {
                var dirst = directiveStatement.Value;

                if (dirst.Type == DirectiveType.Word)
                {
                    statements[i].MemoryLocation = index;
                    index += 2 * dirst.Expressions.Length;
                    continue;
                }

                if (dirst.Type == DirectiveType.Byte)
                {
                    statements[i].MemoryLocation = index;
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
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type == StatementType.Label)
                ResolveLabelValue(statement.Name, i);
        }
    }

    private void ResolveLabelValue(string name, int i)
    {
        int nextIndex = i;
        while (nextIndex < statements.Count)
        {
            int? memoryValue = statements[nextIndex].MemoryLocation;
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

        for (int i = 0; i < statements.Count; i++)
        {
            var instructionStatement = statements[i].InstructionStatement;
            if (instructionStatement != null)
            {
                var istat = instructionStatement.Value;

                if (!resolver.TryResolveEquation(istat.Expression, out int arg))
                    throw new Exception();

                if (istat.AdressingMode == CpuAdressingMode.Relative)
                    arg = arg - statements[i].MemoryLocation!.Value - 2;

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

            var directiveStatement = statements[i].DirectiveStatement;
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
