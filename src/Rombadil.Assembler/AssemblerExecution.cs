namespace Rombadil.Assembler;

internal class AssemblerExecution(
    string[] lines,
    List<AssemblerStatement> statements,
    Dictionary<string, int> declarations,
    Dictionary<string, int> values,
    AssemblerParser parser,
    AssemblerResolver resolver,
    AssemblerAddresser addresser,
    AssemblerEmitter emitter)
{
    internal void Compile()
    {
        ParseLinesIntoStatements();
        DeclareConstants();
        ParseOperationStatements();
        CreateMemoryLayout();
        ResolveLabelValues();
        emitter.Emit();
    }

    private void ParseLinesIntoStatements()
    {
        foreach (var line in lines)
            parser.Parse(line);
    }

    private void DeclareConstants()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var line = statements[i];
            if (line.Type == AssemblerStatementType.Constant || line.Type == AssemblerStatementType.Label)
                declarations.Add(line.Name, i);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type != AssemblerStatementType.Operation)
                continue;

            if (statement.Name.StartsWith('.'))
            {
                if (!Enum.TryParse<AssemblerDirectiveType>(statement.Name[1..], true, out var directiveType))
                    throw new Exception(); // TODO

                var expressions = statement.Value.Split(',');

                statement.Directive = new(directiveType, expressions);
            }
            else
            {
                if (!Enum.TryParse<CpuInstruction>(statement.Name, true, out var instruction))
                    throw new Exception(); // TODO

                var operand = statement.Value;
                var (addressingMode, expression) = addresser.Resolve(instruction, operand);

                statement.Instruction = new(instruction, addressingMode, expression);
            }
        }
    }

    private void CreateMemoryLayout()
    {
        int cur = 0;

        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];

            if (statement.Instruction != null)
            {
                statement.MemoryLocation = cur;
                cur += 1 + CpuAddressingModeSize.Get(statement.Instruction.AddressingMode);
            }
            else if (statement.Directive?.Type == AssemblerDirectiveType.Word)
            {
                statement.MemoryLocation = cur;
                cur += statement.Directive.Expressions.Length * 2;
            }
            else if (statement.Directive?.Type == AssemblerDirectiveType.Byte)
            {
                statement.MemoryLocation = cur;
                cur += statement.Directive.Expressions.Length;
            }
            else if (statement.Directive?.Type == AssemblerDirectiveType.Org)
            {
                if (!resolver.TryResolveEquation(statement.Directive.Expressions[0], out var jmp))
                    throw new Exception(); // TODO

                cur = jmp;
            }
        }
    }

    private void ResolveLabelValues()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type != AssemblerStatementType.Label)
                continue;

            int index = i;
            while (index < statements.Count)
            {
                int? loc = statements[index].MemoryLocation;
                if (loc != null)
                {
                    values.Add(statement.Name, loc.Value);
                    continue;
                }

                index++;
            }

            throw new Exception();
        }
    }
}
