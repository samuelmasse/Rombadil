namespace Rombadil.Assembler;

public class AssemblerLiner
{
    public AssemblerLine[] Process(string[] source)
    {
        int divider = 16;
        int chunk = Math.Max(64, (source.Length / divider) + 1);
        var outputs = new List<AssemblerLine>[divider];

        Parallel.For(0, divider, (i) =>
        {
            int start = i * chunk;
            int end = Math.Min(start + chunk, source.Length);

            if (end > start)
            {
                var section = new AssemblerLinerSection();
                section.Process(source.AsSpan()[start..end]);
                outputs[i] = section.Lines;
            }
            else outputs[i] = [];
        });

        int total = 0;
        foreach (var output in outputs)
            total += output.Count;

        var lines = new AssemblerLine[total];
        int index = 0;
        foreach (var output in outputs)
        {
            output.CopyTo(lines.AsSpan()[index..]);
            index += output.Count;
        }

        return lines;
    }
}
