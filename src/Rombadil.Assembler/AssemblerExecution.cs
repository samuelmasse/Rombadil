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
        ResolveAllConstants();
        emitter.Emit();
    }

    private void ParseLinesIntoStatements()
    {
        for (int i = 0; i < lines.Length; i++)
            parser.Parse(i, lines[i]);
    }

    private void DeclareConstants()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type == AssemblerStatementType.Constant || statement.Type == AssemblerStatementType.Label)
            {
                if (string.IsNullOrEmpty(statement.Name))
                    throw new Assembler6502Exception(statement.LineNumber, $"Names must not be empty.");

                if (!char.IsLetter(statement.Name[0]))
                    throw new Assembler6502Exception(statement.LineNumber, $"Names must begin with a letter \"{statement.Name}\".");

                foreach (char c in statement.Name)
                {
                    if (!char.IsLetterOrDigit(c) && c != '_')
                        throw new Assembler6502Exception(statement.LineNumber,
                            $"Names must be composed only of letters, numbers or underscores \"{statement.Name}\".");
                }

                if (declarations.ContainsKey(statement.Name))
                    throw new Assembler6502Exception(statement.LineNumber, $"Duplicate definition \"{statement.Name}\".");

                declarations.Add(statement.Name, i);
            }
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
                    throw new Assembler6502Exception(statement.LineNumber, $"Unrecognized directive \"{statement.Name}\".");

                var expressions = statement.Value.Split(',');

                statement.Directive = new(directiveType, expressions);
            }
            else
            {
                if (!Enum.TryParse<CpuInstruction>(statement.Name, true, out var instruction))
                    throw new Assembler6502Exception(statement.LineNumber, $"Unrecognized instruction \"{statement.Name}\".");

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
            int? loc = null;

            while (index < statements.Count)
            {
                loc = statements[index].MemoryLocation;
                if (loc != null)
                    break;

                index++;
            }

            if (loc != null)
                values.Add(statement.Name, loc.Value);
            else throw new Assembler6502Exception(statement.LineNumber, $"Dangling label \"{statement.Name}\".");
        }
    }

    private void ResolveAllConstants()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type != AssemblerStatementType.Constant)
                continue;

            if (!resolver.TryResolveEquation(statement.Value, out int value))
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Unable to resolve constant value \"{statement.Value}\" of \"{statement.Name}\".");

            values[statement.Name] = value;
        }
    }
}
