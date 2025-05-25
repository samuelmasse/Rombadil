namespace Rombadil.Assembler;

internal class AssemblerResolver(
    List<AssemblerStatement> statements,
    Dictionary<string, int> declarations,
    Dictionary<string, (int, bool)> values)
{
    private readonly HashSet<string> visited = [];

    internal bool TryResolveEquation(string equation, out (int Value, bool IncludesLabel) value)
    {
        value = (0, false);

        foreach (var term in ParseEquation(equation))
        {
            (int Value, bool IncludesLabel) termValue = (0, false);

            if (char.IsLetter(term.Value[0]))
            {
                if (!TryResolveConstant(term.Value, out termValue))
                    return false;
            }
            else
            {
                if (!TryParseNumber(term.Value, out termValue.Value))
                    return false;
            }

            if (term.Select == EquationTermSelect.LowByte)
                termValue.Value &= 0xFF;
            else if (term.Select == EquationTermSelect.HighByte)
                termValue.Value = (termValue.Value >> 8) & 0xFF;

            if (term.Operation == EquationTermOperation.Add)
                value.Value += termValue.Value;
            else value.Value -= termValue.Value;

            if (termValue.IncludesLabel)
                value.IncludesLabel = true;
        }

        return true;
    }

    private bool TryResolveConstant(string name, out (int, bool) value)
    {
        if (values.TryGetValue(name, out value))
            return true;

        if (!visited.Add(name))
            return false;

        if (!declarations.TryGetValue(name, out var location))
            return false;

        var statement = statements[location];
        if (statement.Type == AssemblerStatementType.Label)
            return false;

        if (!TryResolveEquation(statement.Value, out value))
            return false;

        values.Add(name, value);

        return true;
    }

    private List<EquationTerm> ParseEquation(string expression)
    {
        var result = new List<EquationTerm>();
        int i = 0;
        var op = EquationTermOperation.Add;
        var select = EquationTermSelect.Whole;

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

            if (i < expression.Length)
            {
                if (expression[i] == '<')
                {
                    select = EquationTermSelect.LowByte;
                    i++;
                }
                else if (expression[i] == '>')
                {
                    select = EquationTermSelect.HighByte;
                    i++;
                }
            }

            int start = i;
            while (i < expression.Length && expression[i] != '+' && expression[i] != '-')
                i++;

            if (start < i)
            {
                string value = expression[start..i];

                result.Add(new(value, op, select));

                op = EquationTermOperation.Add;
                select = EquationTermSelect.Whole;
            }
        }

        return result;
    }

    private bool TryParseNumber(string str, out int val)
    {
        try
        {
            if (str.StartsWith('$'))
                val = Convert.ToInt32(str[1..], 16);
            else if (str.StartsWith('%'))
                val = Convert.ToInt32(str[1..], 2);
            else val = Convert.ToInt32(str, 10);

            return true;
        }
        catch
        {
            val = 0;
            return false;
        }
    }

    private record struct EquationTerm(string Value, EquationTermOperation Operation, EquationTermSelect Select);

    private enum EquationTermOperation
    {
        Add,
        Subtract
    }

    private enum EquationTermSelect
    {
        Whole,
        LowByte,
        HighByte
    }
}
