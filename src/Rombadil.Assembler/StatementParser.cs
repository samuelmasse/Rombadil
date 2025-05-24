namespace Rombadil.Assembler;

public class StatementParser
{
    public Statement[] Parse(ReadOnlyMemory<string> lines)
    {
        int divider = 16;
        int chunk = Math.Max(64, (lines.Length / divider) + 1);
        var outputs = new List<Statement>[divider];

        Parallel.For(0, divider, (i) =>
        {
            int start = i * chunk;
            int end = Math.Min(start + chunk, lines.Length);

            if (end > start)
            {
                var section = new StatementParserWorker();
                section.Parse(lines.Span[start..end]);
                outputs[i] = section.Statements;
            }
            else outputs[i] = [];
        });

        int total = 0;
        foreach (var output in outputs)
            total += output.Count;

        var statements = new Statement[total];
        int index = 0;
        foreach (var output in outputs)
        {
            output.CopyTo(statements.AsSpan()[index..]);
            index += output.Count;
        }

        return statements;
    }
}
