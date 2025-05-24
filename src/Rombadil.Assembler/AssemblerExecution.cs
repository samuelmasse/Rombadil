namespace Rombadil.Assembler;

internal class AssemblerExecution(
    string[] lines,
    List<AssemblerStatement> statements,
    AssemblerParser parser,
    AssemblerConstants constants,
    AssemblerResolver resolver,
    AssemblerAddresser addresser,
    AssemblerEmitter emitter)
{
    internal void Compile()
    {
        ParseLines();
        PopulateConstantLocations();
        ParseOperationStatements();
        CreateMemoryLayout();
        ResolveLabelValues();
        emitter.Emit();
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
                statement.MemoryLocation = index;
                index += 1 + CpuAddressingModeSize.Get(statement.Instruction.Value.AddressingMode);
            }
            else if (statement.Directive != null)
            {
                if (statement.Directive.Value.Type == AssemblerDirectiveType.Word)
                {
                    statement.MemoryLocation = index;
                    index += 2 * statement.Directive.Value.Expressions.Length;
                }
                else if (statement.Directive.Value.Type == AssemblerDirectiveType.Byte)
                {
                    statement.MemoryLocation = index;
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
}
