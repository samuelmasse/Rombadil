namespace Rombadil.Assembler;

public class StatementParserWorker
{
    private readonly List<Statement> statements = [];
    private readonly StringBuilder sb = new();

    public List<Statement> Statements => statements;

    public void Parse(ReadOnlySpan<string> lines)
    {
        foreach (string line in lines)
            ParseLine(line);
    }

    private void ParseLine(string source)
    {
        var line = ExtractLabel(CollapseSpaces(RemoveComment(source))).Trim();
        if (string.IsNullOrWhiteSpace(line))
            return;

        if (line.Contains('='))
            ParseConstant(line);
        else ParseOperation(line);
    }

    private void ParseConstant(string str)
    {
        var parts = str.Split('=');

        var name = parts[0].Trim();
        var value = TrimAroundSymbols(parts[1].Trim());

        statements.Add(new(name, value, StatementType.Constant));
    }

    private void ParseOperation(string str)
    {
        int index = str.IndexOf(' ');

        string operation = (index >= 0 ? str[..index] : str).Trim();
        string operand = index >= 0 ? TrimAroundSymbols(str[index..].Trim()) : string.Empty;

        statements.Add(new(operation, operand, StatementType.Operation));
    }

    private string ExtractLabel(string str)
    {
        int index = str.IndexOf(':');
        if (index < 0)
            return str;

        var name = str[..index].Trim();
        statements.Add(new(name, string.Empty, StatementType.Label));

        return str[(index + 1)..];
    }

    private string RemoveComment(string str)
    {
        int comment = str.IndexOf(';');
        return comment >= 0 ? str[..comment] : str;
    }

    private string CollapseSpaces(string str)
    {
        sb.Clear();
        bool inSpace = false;

        foreach (char c in str)
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

    private string TrimAroundSymbols(string str)
    {
        ReadOnlySpan<char> symbols = "()+-#,><";
        sb.Clear();

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            if (c == ' ')
            {
                if (i > 0 && symbols.Contains(str[i - 1]))
                    continue;
                else if (i + 1 < str.Length && symbols.Contains(str[i + 1]))
                    continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
