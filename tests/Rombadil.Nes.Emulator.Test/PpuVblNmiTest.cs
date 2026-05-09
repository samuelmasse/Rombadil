namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class PpuVblNmiTest
{
    // Tests from https://github.com/christopherpow/nes-test-roms/tree/master/ppu_vbl_nmi
    // Tests 07, 08 and 10 don't pass currently

    [TestMethod]
    public void VblBasics() => RunTest("01-vbl_basics");

    [TestMethod]
    public void VblSetTime() => RunTest("02-vbl_set_time");

    [TestMethod]
    public void VblClearTime() => RunTest("03-vbl_clear_time");

    [TestMethod]
    public void NmiControl() => RunTest("04-nmi_control");

    [TestMethod]
    public void NmiTiming() => RunTest("05-nmi_timing");

    [TestMethod]
    public void Suppression() => RunTest("06-suppression");

    [TestMethod]
    public void NmiOnTiming() => RunTest("07-nmi_on_timing",
        """
        00 N
        01 N
        02 N
        03 N
        04 N
        05 N
        06 -
        07 -
        08 -

        2B1F5269
        07-nmi_on_timing

        Failed

        """);

    [TestMethod]
    public void NmiOffTiming() => RunTest("08-nmi_off_timing",
        """
        03 -
        04 -
        05 N
        06 N
        07 N
        08 N
        09 N
        0A N
        0B N
        0C N

        4CC88927
        08-nmi_off_timing

        Failed

        """);

    [TestMethod]
    public void EvenOddFrames() => RunTest("09-even_odd_frames");

    [TestMethod]
    public void EvenOddTiming() => RunTest("10-even_odd_timing",
        $"""
        08 07{' '}
        Clock is skipped too late, relative to enabling BG

        10-even_odd_timing

        Failed #3

        """);

    private void RunTest(string name, string? error = null)
    {
        var rom = File.ReadAllBytes(Path.Join("ppu_vbl_nmi", $"{name}.nes"));

        var prgRom = rom.AsMemory().Slice(0x10, 0x8000);
        var chrRom = rom.AsMemory().Slice(0x8010, 0x2000);

        var mapper = new NesMapperNrom(prgRom, chrRom);
        var state = new CpuEmulatorState();
        var ppu = new NesPpu(mapper, new byte[256 * 240 * 3]);
        var apu = new NesApu(mapper, []);
        var controller1 = new NesController();
        var controller2 = new NesController();
        var bus = new NesMemoryBus(mapper, ppu, apu, controller1, controller2);
        var cpu = new CpuEmulator6502(state, bus);

        cpu.Reset();
        ppu.Reset();

        while (bus[0x6001] != 0xDE || bus[0x6000] > 0x7F)
        {
            bool done = false;
            while (!done)
            {
                cpu.Step();

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
