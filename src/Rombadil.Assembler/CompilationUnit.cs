namespace Rombadil.Assembler;

public class CompilationUnit(string[] source, StatementParser liner)
{
    private readonly Dictionary<string, int> constLocations = [];
    private readonly Dictionary<string, int> constValues = [];

    private Statement[] lines = [];

    public byte[] Compile()
    {
        lines = liner.Parse(source);
        PopulateConstantLocations();
        ParseOperationStatements();
        ResolveAllConstants();
        return [];
    }

    private void PopulateConstantLocations()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Type == StatementType.Constant)
                constLocations.Add(line.Name, i);
        }
    }

    private void ResolveAllConstants()
    {
        foreach (var line in lines)
        {
            if (line.Type == StatementType.Constant)
                ResolveConstant(line.Name);
        }
    }

    private void ParseOperationStatements()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
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

    private int ResolveConstant(string name)
    {
        if (constValues.TryGetValue(name, out var v))
            return v;

        var location = constLocations[name];
        var line = lines[location];

        int sum = ResolveEquation(line.Value);
        constValues.Add(name, sum);

        return sum;
    }

    private int ResolveEquation(string equation)
    {
        var terms = ParseTerms(equation);

        int sum = 0;
        foreach (var term in terms)
        {
            int value = char.IsLetter(term.Value[0]) ?
                ResolveConstant(term.Value) : ParseNumber(term.Value);

            if (term.Operation == EquationTermOperation.Add)
                sum += value;
            else sum -= value;
        }

        return sum;
    }

    public static List<EquationTerm> ParseTerms(string expression)
    {
        var result = new List<EquationTerm>();
        int i = 0;
        var op = EquationTermOperation.Add;

        while (i < expression.Length)
        {
            if (expression[i] == '+')
            {
                op = EquationTermOperation.Add;
                i++;
            }
            else if (expression[i] == '-')
            {
                op = EquationTermOperation.Subtract;
                i++;
            }

            int start = i;
            while (i < expression.Length && expression[i] != '+' && expression[i] != '-')
                i++;

            if (start < i)
            {
                string value = expression[start..i];
                result.Add(new(value, op));
                op = EquationTermOperation.Add;
            }
        }

        return result;
    }

    private int ParseNumber(string expression)
    {
        return 0;
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
