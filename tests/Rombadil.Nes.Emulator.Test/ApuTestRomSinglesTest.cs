namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class ApuTestRomSinglesTest
{
    [TestMethod]
    public void LenCtr() => RunTest("1-len_ctr");

    [TestMethod]
    public void LenTable() => RunTest("2-len_table");

    [TestMethod]
    public void IrqFlag() => RunTest("3-irq_flag");

    [TestMethod]
    public void Jitter() => RunTest("4-jitter");

    [TestMethod]
    public void LenTiming() => RunTest("5-len_timing");

    [TestMethod]
    public void IrqFlagTiming() => RunTest("6-irq_flag_timing");

    [TestMethod]
    public void DmcBasics() => RunTest("7-dmc_basics");

    [TestMethod]
    public void DmcRates() => RunTest("8-dmc_rates");

    private void RunTest(string name, string? error = null)
    {
        var rom = File.ReadAllBytes(Path.Join("apu_test_rom_singles", $"{name}.nes"));

        var prgRom = rom.AsMemory().Slice(0x10, 0x8000);
        var chrRom = rom.AsMemory().Slice(0x8010, 0x2000);

        var mapper = new NesMapperNrom(prgRom, chrRom, NesMirroring.Vertical);
        var state = new CpuEmulatorState();
        var ppu = new NesPpu(mapper, new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 3]);
        var apu = new NesApu(mapper, []);
        var controller1 = new NesController();
        var controller2 = new NesController();
        var bus = new NesMemoryBus(state, mapper, ppu, apu, controller1, controller2);
        var cpu = new CpuEmulator6502(state, bus);

        cpu.Reset();
        ppu.Reset();

        while (bus[0x6001] != 0xDE || bus[0x6000] > 0x7F)
        {
            bool done = false;
            while (!done)
            {
                cpu.Step();

                while (apu.Cycles < state.Cycles)
                    apu.Step();

                if (ppu.PendingNmi)
                {
                    cpu.Nmi();
                    ppu.ClearPendingNmi();
                }

                while (ppu.Cycles < state.Cycles * 3)
                {
                    if (ppu.Step())
                        done = true;
                }
            }
        }

        var result = bus[0x6000];
        string? actualError = null;

        if (result != 0)
        {
            List<byte> b = [];
            int length = 0;
            while (length < 256 && bus[(ushort)(0x6004 + length)] != 0)
            {
                b.Add(bus[(ushort)(0x6004 + length)]);
                length++;
            }

            actualError = Encoding.ASCII.GetString([.. b]);
        }

        error = error?.Replace("\r\n", "\n");
        Assert.AreEqual(error, actualError);
    }
}
