namespace Rombadil;

public class HermiteResampler(Blip blip, double sourceRate, double targetRate)
{
    private readonly ArrayBufferWriter<short> buffer = new();
    private short s0, s1, s2, s3;
    private double srcPos;
    private bool primed;

    public int Read(Span<short> output, double effectiveSpeed)
    {
        if (!primed)
        {
            var initial = buffer.GetSpan(4)[..4];
            int got = blip.ReadSamples(initial);
            if (got < 4)
                return 0;

            s0 = initial[0];
            s1 = initial[1];
            s2 = initial[2];
            s3 = initial[3];

            srcPos = 1.0;
            primed = true;
        }

        double ratio = sourceRate / targetRate * effectiveSpeed;
        int outputCount = output.Length;

        double endSrcPos = srcPos + outputCount * ratio;
        int needed = (int)Math.Floor(endSrcPos) - (int)Math.Floor(srcPos);

        int avail = blip.SamplesAvail;
        if (needed > avail)
            needed = avail;

        var span = buffer.GetSpan(needed)[..needed];
        int srcGot = needed > 0 ? blip.ReadSamples(span) : 0;
        int srcIdx = 0;
        int produced = 0;

        for (int i = 0; i < outputCount; i++)
        {
            while (srcPos >= 2.0)
            {
                if (srcIdx >= srcGot)
                    return produced;

                s0 = s1;
                s1 = s2;
                s2 = s3;
                s3 = span[srcIdx++];
                srcPos -= 1.0;
            }

            double t = srcPos - 1.0;
            double c0 = s1;
            double c1 = 0.5 * (s2 - s0);
            double c2 = s0 - 2.5 * s1 + 2.0 * s2 - 0.5 * s3;
            double c3 = 0.5 * (s3 - s0) + 1.5 * (s1 - s2);
            double y = c0 + t * (c1 + t * (c2 + t * c3));
            output[i] = (short)Math.Clamp(y, short.MinValue, short.MaxValue);
            produced++;

            srcPos += ratio;
        }

        return produced;
    }
}
