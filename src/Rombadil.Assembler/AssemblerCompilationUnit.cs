namespace Rombadil.Assembler;

public class AssemblerCompilationUnit(string[] source, AssemblerLiner liner)
{
    private readonly Dictionary<string, int> constLocations = [];
    private readonly Dictionary<string, int> constValues = [];

    private AssemblerLine[] lines = [];
    private AssemblerInstruction?[] instructions = [];

    public byte[] Compile()
    {
        lines = liner.Process(source);
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
            if (line.Type == AssemblerLineType.Constant)
                constLocations.Add(line.Name, i);
        }
    }

    private void ResolveAllConstants()
    {
        foreach (var line in lines)
        {
            if (line.Type == AssemblerLineType.Constant)
                ResolveConstant(line.Name);
        }
    }

    private void ParseOperationStatements()
    {
        instructions = new AssemblerInstruction?[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Type != AssemblerLineType.Operation)
                continue;

            if (!Enum.TryParse<Instruction>(line.Name, true, out var instruction))
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

    private (AdressingMode, string) ParseOperationArguments(string operand)
    {
        if (operand.Length == 0 || operand == "A" || operand == "a")
            return (AdressingMode.Implied, string.Empty);
        else if (operand.StartsWith('#'))
            return (AdressingMode.Immediate, operand[1..]);
        else if (operand.Contains(','))
        {
            if (operand.StartsWith('(') && operand.EndsWith(",X)", StringComparison.InvariantCultureIgnoreCase))
                return (AdressingMode.IndirectX, operand[1..^3]);
            else if (operand.StartsWith('(') && operand.EndsWith("),Y", StringComparison.InvariantCultureIgnoreCase))
                return (AdressingMode.IndirectY, operand[1..^3]);
            else if (operand.EndsWith(",X", StringComparison.InvariantCultureIgnoreCase))
            {
                // NOTE : if a variable can't be resolved when labels are not defined then it must be absolute. Otherwise it can be zero page if less than max value
                return (AdressingMode.AbsoluteX, operand[..^2]); // Can become ZeroPageX
            }
            else if (operand.EndsWith(",Y", StringComparison.InvariantCultureIgnoreCase))
                return (AdressingMode.AbsoluteY, operand[..^2]); // Can become ZeroPageY
            else throw new Exception($"Invalid operand format: '{operand}'");
        }
        else if (operand.StartsWith('(') && operand.EndsWith(')'))
            return (AdressingMode.Indirect, operand[1..^1]);
        else return (AdressingMode.Absolute, operand); // Can become ZeroPage
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

            if (term.Operation == AssemblerOperation.Add)
                sum += value;
            else sum -= value;
        }

        return sum;
    }

    public static List<AssemblerTerm> ParseTerms(string expression)
    {
        var result = new List<AssemblerTerm>();
        int i = 0;
        var op = AssemblerOperation.Add;

        while (i < expression.Length)
        {
            if (expression[i] == '+')
            {
                op = AssemblerOperation.Add;
                i++;
            }
            else if (expression[i] == '-')
            {
                op = AssemblerOperation.Subtract;
                i++;
            }

            int start = i;
            while (i < expression.Length && expression[i] != '+' && expression[i] != '-')
                i++;

            if (start < i)
            {
                string value = expression[start..i];
                result.Add(new(value, op));
                op = AssemblerOperation.Add;
            }
        }

        return result;
    }

    private int ParseNumber(string expression)
    {
        return 0;
    }

    private void PrintLine(AssemblerLine line)
    {
        if (line.Type == AssemblerLineType.Label)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{line.Name}:");
        }
        else if (line.Type == AssemblerLineType.Constant)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(line.Name);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write('=');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(line.Value);
        }
        else if (line.Type == AssemblerLineType.Operation)
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
