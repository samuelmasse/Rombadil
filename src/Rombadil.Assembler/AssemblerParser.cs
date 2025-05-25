namespace Rombadil.Assembler;

internal class AssemblerParser(List<AssemblerStatement> statements)
{
    private readonly StringBuilder sb = new();

    internal void Parse(int lineNumber, string source)
    {
        var line = ExtractLabel(lineNumber, CollapseSpaces(RemoveComment(source))).Trim();
        if (string.IsNullOrWhiteSpace(line))
            return;

        if (line.Contains('='))
            ParseConstant(lineNumber, line);
        else ParseOperation(lineNumber, line);
    }

    private void ParseConstant(int lineNumber, string str)
    {
        var parts = str.Split('=');

        var name = parts[0].Trim();
        var value = RemoveAllSpaces(parts[1].Trim());

        statements.Add(new(lineNumber, name, value, AssemblerStatementType.Constant));
    }

    private void ParseOperation(int lineNumber, string str)
    {
        int index = str.IndexOf(' ');

        string operation = (index >= 0 ? str[..index] : str).Trim();
        string operand = index >= 0 ? RemoveAllSpaces(str[index..].Trim()) : string.Empty;

        statements.Add(new(lineNumber, operation, operand, AssemblerStatementType.Operation));
    }

    private string ExtractLabel(int lineNumber, string str)
    {
        int index = str.IndexOf(':');
        if (index < 0)
            return str;

        var name = str[..index].Trim();
        statements.Add(new(lineNumber, name, string.Empty, AssemblerStatementType.Label));

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

    private string RemoveAllSpaces(string str)
    {
        sb.Clear();
        foreach (char c in str)
            if (c != ' ')
                sb.Append(c);
        return sb.ToString();
    }
}
