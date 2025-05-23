namespace Rombadil.Assembler;

public class NumberParser
{
    public int Parse(string str)
    {
        if (str.StartsWith('$'))
            return Convert.ToInt32(str[1..], 16);
        if (str.StartsWith('%'))
            return Convert.ToInt32(str[1..], 2);
        return Convert.ToInt32(str, 10);
    }
}
