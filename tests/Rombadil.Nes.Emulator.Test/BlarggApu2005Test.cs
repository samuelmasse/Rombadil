namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class BlarggApu2005Test
{
    private const long CycleLimit = 100_000_000;

    [TestMethod]
    public void LenCtr() => RunTest("01.len_ctr");

    [TestMethod]
    public void LenTable() => RunTest("02.len_table");

    [TestMethod]
    public void IrqFlag() => RunTest("03.irq_flag");

    [TestMethod]
    public void ClockJitter() => RunTest("04.clock_jitter");

    [TestMethod]
    public void LenTimingMode0() => RunTest("05.len_timing_mode0");

    [TestMethod]
    public void LenTimingMode1() => RunTest("06.len_timing_mode1");

    [TestMethod]
    public void IrqFlagTiming() => RunTest("07.irq_flag_timing");

    [TestMethod]
    public void IrqTiming() => RunTest("08.irq_timing");

    [TestMethod]
    public void ResetTiming() => RunTest("09.reset_timing");

    [TestMethod]
    public void LenHaltTiming() => RunTest("10.len_halt_timing");

    [TestMethod]
    public void LenReloadTiming() => RunTest("11.len_reload_timing");

    private void RunTest(string name, byte expected = 1)
    {
        var rom = File.ReadAllBytes(Path.Join("blargg_apu_2005", $"{name}.nes"));

        var header = new NesRomHeader(rom.AsMemory()[..0x10]);
        var prg = rom.AsMemory().Slice(0x10, header.PrgRomSize * 0x4000);
        var chr = rom.AsMemory().Slice(0x10 + prg.Length, header.ChrRomSize * 0x2000);

        var mapper = new NesMapperNrom(prg, chr);
        var state = new CpuEmulatorState();
        var ppu = new NesPpu(mapper, new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 3]);
        var apu = new NesApu(mapper, []);
        var controller1 = new NesController();
        var controller2 = new NesController();
        var bus = new NesMemoryBus(state, mapper, ppu, apu, controller1, controller2);
        var cpu = new CpuEmulator6502(state, bus);

        cpu.Reset();
        ppu.Reset();
        apu.Reset();

        while (bus[0x07F0] != 0xA1)
        {
            cpu.Step();

            while (apu.Cycles < state.Cycles)
                apu.Step();

            if (ppu.PendingNmi)
            {
                cpu.Nmi();
                ppu.ClearPendingNmi();
            }

            if (apu.PendingIrq)
                cpu.Irq();

            while (ppu.Cycles < state.Cycles * 3)
                ppu.Step();

            if (state.Cycles > CycleLimit)
                Assert.Fail($"{name}: test did not complete within {CycleLimit} cycles");
        }

        Assert.AreEqual(expected, bus[0x00F0], $"{name}: result code");
    }
}
