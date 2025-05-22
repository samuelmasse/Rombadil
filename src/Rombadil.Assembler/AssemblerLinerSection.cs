namespace Rombadil.Assembler;

public class AssemblerLinerSection
{
    private readonly List<AssemblerLine> lines = [];
    private readonly StringBuilder sb = new();

    public List<AssemblerLine> Lines => lines;

    public void Process(Span<string> source)
    {
        foreach (string s in source)
            ProcessLine(s);
    }

    private void ProcessLine(string source)
    {
        var line = ExtractLabel(CollapseSpaces(RemoveComment(source))).Trim();
        if (string.IsNullOrWhiteSpace(line))
            return;

        if (line.Contains('='))
            ProcessConstant(line);
        else ProcessOperation(line);
    }

    private void ProcessConstant(string source)
    {
        var parts = source.Split('=');

        var name = parts[0].Trim();
        var value = TrimAroundSymbols(parts[1].Trim());

        lines.Add(new(name, value, AssemblerLineType.Constant));
    }

    private void ProcessOperation(string source)
    {
        int index = source.IndexOf(' ');

        string operation = (index >= 0 ? source[..index] : source).Trim();
        string operand = index >= 0 ? TrimAroundSymbols(source[index..].Trim()) : string.Empty;

        lines.Add(new(operation, operand, AssemblerLineType.Operation));
    }

    private string ExtractLabel(string source)
    {
        int index = source.IndexOf(':');
        if (index < 0)
            return source;

        var name = source[..index].Trim();
        lines.Add(new(name, string.Empty, AssemblerLineType.Label));

        return source[(index + 1)..];
    }

    private string RemoveComment(string source)
    {
        int comment = source.IndexOf(';');
        return comment >= 0 ? source[..comment] : source;
    }

    private string CollapseSpaces(string input)
    {
        sb.Clear();
        bool inSpace = false;

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!inSpace)
                {
                    sb.Append(' ');
                    inSpace = true;
                }
            }
            else
            {
                sb.Append(c);
                inSpace = false;
            }
        }

        return sb.ToString();
    }

    private string TrimAroundSymbols(string input)
    {
        ReadOnlySpan<char> symbols = "()+-#,><";
        sb.Clear();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == ' ')
            {
                if (i > 0 && symbols.Contains(input[i - 1]))
                    continue;
                else if (i + 1 < input.Length && symbols.Contains(input[i + 1]))
                    continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
