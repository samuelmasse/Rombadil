namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class PpuVblNmiTest
{
    [TestMethod]
    [DataRow("01-vbl_basics")]
    [DataRow("02-vbl_set_time")]
    [DataRow("03-vbl_clear_time")]
    [DataRow("04-nmi_control")]
    [DataRow("05-nmi_timing")]
    [DataRow("06-suppression")]
    [DataRow("07-nmi_on_timing")]
    [DataRow("08-nmi_off_timing")]
    [DataRow("09-even_odd_frames")]
    [DataRow("10-even_odd_timing")]
    public void RomPasses(string name) => BlarggStatusTest.AssertPassed("ppu_vbl_nmi", name);
}
