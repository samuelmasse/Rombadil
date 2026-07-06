namespace Rombadil;

public static class RombadilNesTiming
{
    public const double CpuHz = 1789773;

    public static readonly long CyclesPerFrame = (long)Math.Ceiling(CpuHz / 60);
}
