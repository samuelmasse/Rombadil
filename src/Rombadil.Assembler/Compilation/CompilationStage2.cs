namespace Rombadil.Assembler;

public class CompilationStage2(CompilationStatements statements, CompilationConstants constants, CompilationResolver resolver)
{
    public byte[] Compile()
    {
        PopulateConstantLocations();
        ResolveAllConstants();
        ParseOperationStatements();
        return [];
    }

    private void PopulateConstantLocations()
    {
        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var line = statements.Statements[i];
            if (line.Type == StatementType.Constant)
                constants.Declare(line.Name, i);
        }
    }

    private void ResolveAllConstants()
    {
        foreach (var statement in statements.Statements)
        {
            if (statement.Type == StatementType.Constant)
                resolver.ResolveConstant(statement.Name);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < statements.Statements.Length; i++)
        {
            var line = statements.Statements[i];
            if (line.Type != StatementType.Operation)
                continue;

            if (!Enum.TryParse<CpuInstruction>(line.Name, true, out var instruction))
            {
                // .org must expect constant expression
                // Console.WriteLine(line.Name);
            }
            else
            {
                var operand = line.Value;
                var (mode, arg) = ParseOperationArguments(operand);

                PrintLine(line);
                Console.WriteLine($"{instruction} {mode} {arg}");
            }
        }
    }

    private (CpuAdressingMode, string) ParseOperationArguments(string operand)
    {
        if (operand.Length == 0 || operand == "A" || operand == "a")
            return (CpuAdressingMode.Implied, string.Empty);
        else if (operand.StartsWith('#'))
            return (CpuAdressingMode.Immediate, operand[1..]);
        else if (operand.Contains(','))
        {
            if (operand.StartsWith('(') && operand.EndsWith(",X)", StringComparison.InvariantCultureIgnoreCase))
                return (CpuAdressingMode.IndirectX, operand[1..^3]);
            else if (operand.StartsWith('(') && operand.EndsWith("),Y", StringComparison.InvariantCultureIgnoreCase))
                return (CpuAdressingMode.IndirectY, operand[1..^3]);
            else if (operand.EndsWith(",X", StringComparison.InvariantCultureIgnoreCase))
            {
                // NOTE : if a variable can't be resolved when labels are not defined then it must be absolute.
                // Otherwise it can be zero page if less than max value
                return (CpuAdressingMode.AbsoluteX, operand[..^2]); // Can become ZeroPageX
            }
            else if (operand.EndsWith(",Y", StringComparison.InvariantCultureIgnoreCase))
                return (CpuAdressingMode.AbsoluteY, operand[..^2]); // Can become ZeroPageY
            else throw new Exception($"Invalid operand format: '{operand}'");
        }
        else if (operand.StartsWith('(') && operand.EndsWith(')'))
            return (CpuAdressingMode.Indirect, operand[1..^1]);
        else return (CpuAdressingMode.Absolute, operand); // Can become ZeroPage
    }

    private void PrintLine(Statement line)
    {
        if (line.Type == StatementType.Label)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{line.Name}:");
        }
        else if (line.Type == StatementType.Constant)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(line.Name);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write('=');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(line.Value);
        }
        else if (line.Type == StatementType.Operation)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(line.Name);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(line.Value);
        }

        Console.ResetColor();
    }
}
