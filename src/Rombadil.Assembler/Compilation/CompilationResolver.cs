namespace Rombadil.Assembler;

public class CompilationResolver(
    CompilationStatements statements,
    CompilationConstants constants,
    EquationParser equationParser,
    NumberParser numberParser)
{
    public bool TryResolveConstant(string name, out int value)
    {
        if (constants.TryGetValue(name, out value))
            return true;

        if (!constants.TryGetStatementIndex(name, out var location))
            return false;

        var statement = statements.Statements[location];
        if (statement.Type == StatementType.Label)
            return false;

        if (!TryResolveEquation(statement.Value, out value))
            return false;

        constants.SetValue(name, value);

        return true;
    }

    public bool TryResolveEquation(string equation, out int value)
    {
        var terms = equationParser.Parse(equation);
        value = 0;

        foreach (var term in terms)
        {
            int val;

            if (char.IsLetter(term.Value[0]))
            {
                if (!TryResolveConstant(term.Value, out val))
                    return false;
            }
            else val = numberParser.Parse(term.Value);

            if (term.Select == EquationTermSelect.LowByte)
                val &= 0xFF;
            else if (term.Select == EquationTermSelect.HighByte)
                val = (val >> 8) & 0xFF;

            if (term.Operation == EquationTermOperation.Add)
                value += val;
            else value -= val;
        }

        return true;
    }
}
