namespace Rombadil.Assembler;

internal class AssemblerExecution(
    string[] lines,
    List<AssemblerStatement> statements,
    AssemblerParser parser,
    AssemblerConstants constants,
    AssemblerResolver resolver,
    AssemblerAddresser addresser)
{
    internal byte[] Compile()
    {
        ParseLines();
        PopulateConstantLocations();
        ParseOperationStatements();
        CreateMemoryLayout();
        ResolveLabelValues();
        return EmitBinary();
    }

    private void ParseLines()
    {
        foreach (var line in lines)
            parser.Parse(line);
    }

    private void PopulateConstantLocations()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var line = statements[i];
            if (line.Type == AssemblerStatementType.Constant || line.Type == AssemblerStatementType.Label)
                constants.Declare(line.Name, i);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type == AssemblerStatementType.Operation)
                ParseOperationStatement(statement, i);
        }
    }

    private void ParseOperationStatement(AssemblerStatement statement, int i)
    {
        if (statement.Name.StartsWith('.'))
            ParseDirectiveStatement(statement, i);
        else ParseInstructionStatement(statement, i);
    }

    private void ParseDirectiveStatement(AssemblerStatement statement, int i)
    {
        if (!Enum.TryParse<AssemblerDirectiveType>(statement.Name[1..], true, out var directiveType))
            throw new Exception(); // TODO

        var expressions = statement.Value.Split(',');
        statements[i].Directive = new(directiveType, expressions);
    }

    private void ParseInstructionStatement(AssemblerStatement statement, int i)
    {
        if (!Enum.TryParse<CpuInstruction>(statement.Name, true, out var instruction))
            throw new Exception(); // TODO

        var operand = statement.Value;
        var (addressingMode, expression) = addresser.Resolve(instruction, operand);
        statements[i].Instruction = new(instruction, addressingMode, expression);
    }

    private void CreateMemoryLayout()
    {
        int index = 0;

        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];

            if (statement.Instruction != null)
            {
                statements[i].MemoryLocation = index;
                index += 1 + OperandSize(statement.Instruction.Value.AddressingMode);
                continue;
            }
            else if (statement.Directive != null)
            {
                if (statement.Directive.Value.Type == AssemblerDirectiveType.Word)
                {
                    statements[i].MemoryLocation = index;
                    index += 2 * statement.Directive.Value.Expressions.Length;
                }
                else if (statement.Directive.Value.Type == AssemblerDirectiveType.Byte)
                {
                    statements[i].MemoryLocation = index;
                    index += statement.Directive.Value.Expressions.Length;
                }
                else if (statement.Directive.Value.Type == AssemblerDirectiveType.Org)
                {
                    if (!resolver.TryResolveEquation(statement.Directive.Value.Expressions[0], out var jmp))
                        throw new Exception(); // TODO

                    index = jmp;
                }
            }
        }
    }

    private void ResolveLabelValues()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type == AssemblerStatementType.Label)
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
            var instructionStatement = statements[i].Instruction;
            if (instructionStatement != null)
            {
                var istat = instructionStatement.Value;

                if (!resolver.TryResolveEquation(istat.Expression, out int arg))
                    throw new Exception();

                if (istat.AddressingMode == CpuAddressingMode.Relative)
                    arg = arg - statements[i].MemoryLocation!.Value - 2;

                if (!CpuOpcodeMap.TryEncodeOpcode(istat.Instruction, istat.AddressingMode, out var opcode))
                    throw new InvalidOperationException($"No opcode found for {istat.Instruction} with {istat.AddressingMode} addressing");
                output.Add((byte)opcode);

                var size = OperandSize(istat.AddressingMode);
                if (size >= 1)
                    output.Add((byte)(arg & 0xFF));
                if (size >= 2)
                    output.Add((byte)((arg >> 8) & 0xFF));

                continue;
            }

            var directiveStatement = statements[i].Directive;
            if (directiveStatement != null)
            {
                var dstat = directiveStatement.Value;

                if (dstat.Type == AssemblerDirectiveType.Byte)
                {
                    foreach (var expression in dstat.Expressions)
                    {
                        if (!resolver.TryResolveEquation(expression, out int val))
                            throw new Exception();

                        output.Add((byte)(val & 0xFF));
                    }
                }
                else if (dstat.Type == AssemblerDirectiveType.Word)
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

    private static int OperandSize(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Indirect ||
            mode == CpuAddressingMode.Absolute ||
            mode == CpuAddressingMode.AbsoluteX ||
            mode == CpuAddressingMode.AbsoluteY)
            return 2;
        else if (mode == CpuAddressingMode.Implied)
            return 0;
        else return 1;
    }
}
