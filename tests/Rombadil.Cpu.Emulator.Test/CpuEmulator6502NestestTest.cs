namespace Rombadil.Cpu.Emulator.Test;

[TestClass]
public sealed class CpuEmulator6502NestestTest
{
    [TestMethod]
    public void Nestest()
    {
        var bytes = new byte[0x10000];
        for (int i = 0; i <= 0x17; i++)
            bytes[0x4000 + i] = 0xFF;

        var map = new ushort[0x10000];
        for (int i = 0; i < map.Length; i++)
            map[i] = (ushort)i;
        for (int i = 0; i < 0x4000; i++)
            map[0xC000 + i] = (ushort)(0x8000 + i);

        // The log can also be validated by running and comparing the log
        // em65 nestest.nes -s 0x10 -l 0x4000 -m 0x8000 -r 0xC000,0x4000=0x8000 -p 0xC000 -b 0x4000,0x17=0xFF > nestest.log

        var rom = File.ReadAllBytes("nestest.nes");
        var log = File.ReadAllLines("nestest.log");
        var prgArea = bytes.AsSpan().Slice(0x8000, 0x4000);
        var prgRom = rom.AsSpan().Slice(0x10, 0x4000);
        prgRom.CopyTo(prgArea);

        var state = new CpuEmulatorState();
        var bus = new CpuEmulatorBusMap(bytes, map);
        var cpu = new CpuEmulator6502(state, bus);
        var logger = new CpuEmulatorLogger(state, bus, cpu);
        cpu.Reset(0xC000);

        for (int i = 0; i < log.Length; i++)
        {
            string msg = logger.Log();
            if (log[i] != msg)
                throw new AssertFailedException($"\n{i + 1,-8} Expected : {log[i]}\n{i + 1,-8} But got  : {msg}");

            cpu.Step();
        }
    }
}
