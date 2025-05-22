namespace Rombadil.Assembler;

public class EquationParser
{
    public List<EquationTerm> Parse(string expression)
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
}
