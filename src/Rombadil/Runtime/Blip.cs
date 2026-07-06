namespace Rombadil;

public class Blip
{
    private const int PreShift = 32;
    private const int TimeBits = PreShift + 20;
    private const ulong TimeUnit = 1UL << TimeBits;
    private const int BassShift = 9;
    private const int EndFrameExtra = 2;
    private const int HalfWidth = 8;
    private const int BufExtra = HalfWidth * 2 + EndFrameExtra;
    private const int PhaseBits = 5;
    private const int PhaseCount = 1 << PhaseBits;
    private const int DeltaBits = 15;
    private const int DeltaUnit = 1 << DeltaBits;
    private const int FracBits = TimeBits - PreShift;
    private const int PhaseShift = FracBits - PhaseBits;
    private const int MaxSample = 32767;

    private static readonly short[,] BlStep =
    {
        {  43, -115,  350, -488, 1136, -914, 5861, 21022 },
        {  44, -118,  348, -473, 1076, -799, 5274, 21001 },
        {  45, -121,  344, -454, 1011, -677, 4706, 20936 },
        {  46, -122,  336, -431,  942, -549, 4156, 20829 },
        {  47, -123,  327, -404,  868, -418, 3629, 20679 },
        {  47, -122,  316, -375,  792, -285, 3124, 20488 },
        {  47, -120,  303, -344,  714, -151, 2644, 20256 },
        {  46, -117,  289, -310,  634,  -17, 2188, 19985 },
        {  46, -114,  273, -275,  553,  117, 1758, 19675 },
        {  44, -108,  255, -237,  471,  247, 1356, 19327 },
        {  43, -103,  237, -199,  390,  373,  981, 18944 },
        {  42,  -98,  218, -160,  310,  495,  633, 18527 },
        {  40,  -91,  198, -121,  231,  611,  314, 18078 },
        {  38,  -84,  178,  -81,  153,  722,   22, 17599 },
        {  36,  -76,  157,  -43,   80,  824, -241, 17092 },
        {  34,  -68,  135,   -3,    8,  919, -476, 16558 },
        {  32,  -61,  115,   34,  -60, 1006, -683, 16001 },
        {  29,  -52,   94,   70, -123, 1083, -862, 15422 },
        {  27,  -44,   73,  106, -184, 1152,-1015, 14824 },
        {  25,  -36,   53,  139, -239, 1211,-1142, 14210 },
        {  22,  -27,   34,  170, -290, 1261,-1244, 13582 },
        {  20,  -20,   16,  199, -335, 1301,-1322, 12942 },
        {  18,  -12,   -3,  226, -375, 1331,-1376, 12293 },
        {  15,   -4,  -19,  250, -410, 1351,-1408, 11638 },
        {  13,    3,  -35,  272, -439, 1361,-1419, 10979 },
        {  11,    9,  -49,  292, -464, 1362,-1410, 10319 },
        {   9,   16,  -63,  309, -483, 1354,-1383,  9660 },
        {   7,   22,  -75,  322, -496, 1337,-1339,  9005 },
        {   6,   26,  -85,  333, -504, 1312,-1280,  8355 },
        {   4,   31,  -94,  341, -507, 1278,-1205,  7713 },
        {   3,   35, -102,  347, -506, 1238,-1119,  7082 },
        {   1,   40, -110,  350, -499, 1190,-1021,  6464 },
        {   0,   43, -115,  350, -488, 1136, -914,  5861 },
    };

    private readonly int[] buffer;
    private readonly ulong factor;
    private ulong offset;
    private int avail;
    private int integrator;

    public int SamplesAvail => avail;

    public Blip(int size, double clockRate, double sampleRate)
    {
        buffer = new int[size + BufExtra];

        double f = TimeUnit * sampleRate / clockRate;
        factor = (ulong)Math.Ceiling(f);

        Clear();
    }

    public void Clear()
    {
        offset = factor / 2;
        avail = 0;
        integrator = 0;
        Array.Clear(buffer, 0, buffer.Length);
    }

    public void EndFrame(uint t)
    {
        ulong off = t * factor + offset;
        avail += (int)(off >> TimeBits);
        offset = off & (TimeUnit - 1);
    }

    public void AddDelta(uint time, int delta)
    {
        uint fixedPos = (uint)((time * factor + offset) >> PreShift);
        int outBase = avail + (int)(fixedPos >> FracBits);
        int phase = (int)(fixedPos >> PhaseShift) & (PhaseCount - 1);
        int rev = PhaseCount - phase;
        int interp = (int)(fixedPos >> (PhaseShift - DeltaBits)) & (DeltaUnit - 1);
        int delta2 = (delta * interp) >> DeltaBits;
        delta -= delta2;

        buffer[outBase + 0] += BlStep[phase, 0] * delta + BlStep[phase + 1, 0] * delta2;
        buffer[outBase + 1] += BlStep[phase, 1] * delta + BlStep[phase + 1, 1] * delta2;
        buffer[outBase + 2] += BlStep[phase, 2] * delta + BlStep[phase + 1, 2] * delta2;
        buffer[outBase + 3] += BlStep[phase, 3] * delta + BlStep[phase + 1, 3] * delta2;
        buffer[outBase + 4] += BlStep[phase, 4] * delta + BlStep[phase + 1, 4] * delta2;
        buffer[outBase + 5] += BlStep[phase, 5] * delta + BlStep[phase + 1, 5] * delta2;
        buffer[outBase + 6] += BlStep[phase, 6] * delta + BlStep[phase + 1, 6] * delta2;
        buffer[outBase + 7] += BlStep[phase, 7] * delta + BlStep[phase + 1, 7] * delta2;
        buffer[outBase + 8] += BlStep[rev, 7] * delta + BlStep[rev - 1, 7] * delta2;
        buffer[outBase + 9] += BlStep[rev, 6] * delta + BlStep[rev - 1, 6] * delta2;
        buffer[outBase + 10] += BlStep[rev, 5] * delta + BlStep[rev - 1, 5] * delta2;
        buffer[outBase + 11] += BlStep[rev, 4] * delta + BlStep[rev - 1, 4] * delta2;
        buffer[outBase + 12] += BlStep[rev, 3] * delta + BlStep[rev - 1, 3] * delta2;
        buffer[outBase + 13] += BlStep[rev, 2] * delta + BlStep[rev - 1, 2] * delta2;
        buffer[outBase + 14] += BlStep[rev, 1] * delta + BlStep[rev - 1, 1] * delta2;
        buffer[outBase + 15] += BlStep[rev, 0] * delta + BlStep[rev - 1, 0] * delta2;
    }

    public int ReadSamples(Span<short> output)
    {
        int count = output.Length;

        if (count > avail)
            count = avail;

        if (count == 0)
            return 0;

        int sum = integrator;

        for (int i = 0; i < count; i++)
        {
            int s = sum >> DeltaBits;
            sum += buffer[i];
            if ((short)s != s)
                s = (s >> 16) ^ MaxSample;
            output[i] = (short)s;
            sum -= s << (DeltaBits - BassShift);
        }

        integrator = sum;
        RemoveSamples(count);
        return count;
    }

    private void RemoveSamples(int count)
    {
        int remain = avail + BufExtra - count;
        avail -= count;
        Array.Copy(buffer, count, buffer, 0, remain);
        Array.Clear(buffer, remain, count);
    }
}
