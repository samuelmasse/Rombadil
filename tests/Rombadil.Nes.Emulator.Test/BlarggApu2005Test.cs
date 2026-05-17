namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class BlarggApu2005Test
{
    private const int MaxFrames = 4000;

    [TestMethod]
    [DataRow("01.len_ctr")]
    [DataRow("02.len_table")]
    [DataRow("03.irq_flag")]
    [DataRow("04.clock_jitter")]
    [DataRow("05.len_timing_mode0")]
    [DataRow("06.len_timing_mode1")]
    [DataRow("07.irq_flag_timing")]
    [DataRow("08.irq_timing")]
    [DataRow("09.reset_timing")]
    [DataRow("10.len_halt_timing")]
    [DataRow("11.len_reload_timing")]
    public void RomPasses(string name)
    {
        var runner = NesTestRomRunner.Load("blargg_apu_2005", name);
        runner.RunUntil(
            completed: r => r.Peek(0x07F0) == 0xA1,
            MaxFrames,
            timeoutMessage: $"{name}: test did not complete within {MaxFrames} frames");

        Assert.AreEqual((byte)1, runner.Peek(0x00F0), $"{name}: result code");
    }
}
