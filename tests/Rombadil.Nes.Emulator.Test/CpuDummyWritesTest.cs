namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class CpuDummyWritesTest
{
    [TestMethod]
    [DataRow("cpu_dummy_writes_oam")]
    [DataRow("cpu_dummy_writes_ppumem")]
    public void RomPasses(string name) => BlarggStatusTest.AssertPassed("cpu_dummy_writes", name, outputLength: 0x1FFC);
}
