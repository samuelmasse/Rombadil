namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class ApuTestRomSinglesTest
{
    [TestMethod]
    [DataRow("1-len_ctr")]
    [DataRow("2-len_table")]
    [DataRow("3-irq_flag")]
    [DataRow("4-jitter")]
    [DataRow("5-len_timing")]
    [DataRow("6-irq_flag_timing")]
    [DataRow("7-dmc_basics")]
    [DataRow("8-dmc_rates")]
    public void RomPasses(string name) => BlarggStatusTest.AssertPassed("apu_test_rom_singles", name);
}
