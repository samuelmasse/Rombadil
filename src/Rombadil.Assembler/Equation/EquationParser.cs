namespace Rombadil.Assembler;

public class EquationParser
{
    public List<EquationTerm> Parse(string expression)
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
}
