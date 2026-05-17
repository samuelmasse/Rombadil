namespace Rombadil.Nes.Emulator.Test;

internal static class BlarggStatusTest
{
    private const int DefaultMaxFrames = 1800;

    public static void AssertPassed(
        string directory,
        string name,
        int maxFrames = DefaultMaxFrames,
        int outputLength = 256,
        string? expectedError = null)
    {
        var runner = NesTestRomRunner.Load(directory, name);
        runner.RunUntil(
            completed: r => r.Peek(0x6001) == 0xDE && r.Peek(0x6000) <= 0x7F,
            maxFrames,
            timeoutMessage: $"{name}: test did not complete within {maxFrames} frames\n{runner.ReadAscii(0x6004, outputLength)}");

        byte result = runner.Peek(0x6000);
        string? actualError = result == 0 ? null : runner.ReadAscii(0x6004, outputLength);
        Assert.AreEqual(expectedError?.Replace("\r\n", "\n"), actualError);
    }
}
