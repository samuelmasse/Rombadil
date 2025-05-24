namespace Rombadil.Assembler;

internal class AssemblerResolver(List<AssemblerStatement> statements, AssemblerConstants constants)
{
    internal bool TryResolveEquation(string equation, out int value)
    {
        value = 0;

        foreach (var term in ParseEquation(equation))
        {
            int termValue;

            if (char.IsLetter(term.Value[0]))
            {
                if (!TryResolveConstant(term.Value, out termValue))
                    return false;
            }
            else termValue = ParseNumber(term.Value);

            if (term.Select == EquationTermSelect.LowByte)
                termValue &= 0xFF;
            else if (term.Select == EquationTermSelect.HighByte)
                termValue = (termValue >> 8) & 0xFF;

            if (term.Operation == EquationTermOperation.Add)
                value += termValue;
            else value -= termValue;
        }

        return true;
    }

    private bool TryResolveConstant(string name, out int value)
    {
        if (constants.TryGetValue(name, out value))
            return true;

        if (!constants.TryGetStatementIndex(name, out var location))
            return false;

        var statement = statements[location];
        if (statement.Type == AssemblerStatementType.Label)
            return false;

        if (!TryResolveEquation(statement.Value, out value))
            return false;

        constants.SetValue(name, value);

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

    private int ParseNumber(string str)
    {
        if (str.StartsWith('$'))
            return Convert.ToInt32(str[1..], 16);
        if (str.StartsWith('%'))
            return Convert.ToInt32(str[1..], 2);
        return Convert.ToInt32(str, 10);
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
