namespace Rombadil.Assembler;

internal class AssemblerExecution(
    IFileSystem fileSystem,
    IReadOnlyList<AssemblerSegment> segments,
    string[] lines,
    List<AssemblerStatement> statements,
    Dictionary<string, int> declarations,
    Dictionary<string, (int, bool)> values,
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
                string name = statement.Name;

                if (string.IsNullOrWhiteSpace(name))
                    throw new Assembler6502Exception(statement.LineNumber, "Name cannot be empty.");

                if (!char.IsLetter(name[0]))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Name \"{name}\" must begin with a letter.");

                foreach (char c in name)
                {
                    if (!char.IsLetterOrDigit(c) && c != '_')
                        throw new Assembler6502Exception(statement.LineNumber,
                            $"Name \"{name}\" contains an invalid character '{c}'. Only letters, digits, and underscores are allowed.");
                }

                if (declarations.ContainsKey(name))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Duplicate symbol \"{name}\" is already defined.");

                declarations.Add(name, i);
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
            else if (statement.Directive?.Type == AssemblerDirectiveType.Incbin)
            {
                if (statement.Directive.Expressions.Length != 1)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"The \".incbin\" directive requires exactly one file path argument.");

                var expression = statement.Directive.Expressions[0];
                if (!expression.StartsWith('"') || !expression.EndsWith('"'))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"The argument to \".incbin\" must be a quoted file path, e.g., '.incbin \"file.bin\"'.");

                var path = expression[1..^1];
                if (!fileSystem.File.Exists(path))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"The file \"{path}\" specified in \".incbin\" was not found.");

                var bytes = fileSystem.File.ReadAllBytes(path);

                statement.IncludedBytes = bytes;
                statement.MemoryLocation = cur;
                cur += bytes.Length;
            }
            else if (statement.Directive?.Type == AssemblerDirectiveType.Segment)
            {
                if (statement.Directive.Expressions.Length != 1)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"The \".segment\" directive requires exactly one segment name argument.");

                var expression = statement.Directive.Expressions[0];
                if (!expression.StartsWith('"') && !expression.EndsWith('"'))
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"The argument to \".segment\" must be a quoted segment name, e.g., '.segment \"CODE\"'.");

                var name = expression[1..^1];
                var segment = segments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (segment == null)
                    throw new Assembler6502Exception(statement.LineNumber,
                        $"Segment \"{name}\" not defined. Available segments: {string.Join(", ", segments.Select(s => $"\"{s.Name}\""))}");

                statement.Segment = segment;
                cur = segment.MemoryStart;
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
                values.Add(statement.Name, (loc.Value, true));
            else throw new Assembler6502Exception(statement.LineNumber,
                $"Label \"{statement.Name}\" is not followed by any addressable statement.");
        }
    }

    private void ResolveAllConstants()
    {
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            if (statement.Type != AssemblerStatementType.Constant)
                continue;

            if (!resolver.TryResolveEquation(statement.Value, out var value))
                throw new Assembler6502Exception(statement.LineNumber,
                    $"Could not evaluate expression \"{statement.Value}\" for constant \"{statement.Name}\".");

            values[statement.Name] = value;
        }
    }
}
