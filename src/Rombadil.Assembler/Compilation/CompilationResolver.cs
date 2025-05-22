namespace Rombadil.Assembler;

public class CompilationResolver(
    CompilationStatements statements,
    CompilationConstants constants,
    EquationParser equationParser,
    NumberParser numberParser)
{
    public int ResolveConstant(string name)
    {
        if (constants.TryGetValue(name, out var v))
            return v;

        constants.TryGetStatementIndex(name, out var location);
        var statement = statements.Statements[location];

        int sum = ResolveEquation(statement.Value);
        constants.SetValue(name, sum);

        return sum;
    }

    public int ResolveEquation(string equation)
    {
        var terms = equationParser.Parse(equation);

        int sum = 0;
        foreach (var term in terms)
        {
            int value = char.IsLetter(term.Value[0]) ?
                ResolveConstant(term.Value) : numberParser.Parse(term.Value);

            if (term.Operation == EquationTermOperation.Add)
                sum += value;
            else sum -= value;
        }

        return sum;
    }
}
