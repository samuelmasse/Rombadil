namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class BatterySaveTest
{
    [TestMethod]
    public void BatteryRam_TracksChangesAndLoadsCleanly()
    {
        var ram = new NesBatteryRam(4);

        ram.Write(1, 7);

        Assert.IsTrue(ram.Dirty);
        CollectionAssert.AreEqual(new byte[] { 0, 7, 0, 0 }, Copy(ram));

        ram.MarkClean();
        ram.Write(1, 7);
        Assert.IsFalse(ram.Dirty);

        ram.Load(new byte[] { 1, 2, 3, 4 });
        Assert.IsFalse(ram.Dirty);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, Copy(ram));
    }

    [TestMethod]
    public void Header_SeparatesInesBatteryRam()
    {
        byte[] rom = CreateRom(mapper: 0, battery: true, prgRamUnits: 1);
        var header = new NesRomHeader(rom);

        Assert.AreEqual(0, header.PrgRamSize);
        Assert.AreEqual(0x2000, header.PrgNvRamSize);
        Assert.AreEqual(0x2000, header.ChrRamSize);
        Assert.AreEqual(0, header.ChrNvRamSize);
    }

    [TestMethod]
    public void Header_DecodesNes20PersistentRam()
    {
        byte[] rom = CreateRom(mapper: 0, battery: true, prgRamUnits: 0);
        rom[7] = 0x08;
        rom[10] = 0x87;
        rom[11] = 0x65;
        var header = new NesRomHeader(rom);

        Assert.AreEqual(0x2000, header.PrgRamSize);
        Assert.AreEqual(0x4000, header.PrgNvRamSize);
        Assert.AreEqual(0x0800, header.ChrRamSize);
        Assert.AreEqual(0x1000, header.ChrNvRamSize);
    }

    [TestMethod]
    public void Emulator_ResetPreservesBatteryRam()
    {
        byte[] rom = CreateRom(mapper: 0, battery: true, prgRamUnits: 1);
        int prg = 0x10;
        rom[prg + 0] = 0xA9;
        rom[prg + 1] = 0x42;
        rom[prg + 2] = 0x8D;
        rom[prg + 3] = 0x00;
        rom[prg + 4] = 0x60;
        rom[prg + 5] = 0x4C;
        rom[prg + 6] = 0x05;
        rom[prg + 7] = 0x80;
        rom[prg + 0x3FFC] = 0x00;
        rom[prg + 0x3FFD] = 0x80;

        var emulator = new NesEmulator(rom, new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 4], []);
        emulator.Step(20);
        Assert.IsTrue(emulator.BatterySaveDirty);

        emulator.Reset();
        var save = new byte[emulator.BatterySaveSize];
        emulator.CopyBatterySave(save);

        Assert.AreEqual((byte)0x42, save[0]);
    }

    [TestMethod]
    public void ChrNvRam_FollowsPrgNvRamInSnapshot()
    {
        var mapper = new NesMapperNrom(
            new byte[0x4000],
            Memory<byte>.Empty,
            NesMirroring.Horizontal,
            new NesCartridgeRamSizes(0, 2, 0, 2));
        mapper.WritePrgRam(0x6000, 0x11);
        mapper.WriteChr(0, 0x22);
        var save = new byte[mapper.BatterySaveSize];

        mapper.CopyBatterySave(save);

        CollectionAssert.AreEqual(new byte[] { 0x11, 0, 0x22, 0 }, save);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(7)]
    [DataRow(9)]
    [DataRow(23)]
    [DataRow(25)]
    [DataRow(148)]
    public void SupportedMapper_ExposesDeclaredBatteryRam(int mapperNumber)
    {
        var prg = new byte[0x80000];
        var chr = new byte[0x2000];
        var ram = new NesCartridgeRamSizes(0, 0x2000, 0, 0);
        NesMapper mapper = mapperNumber switch
        {
            0 => new NesMapperNrom(prg, chr, NesMirroring.Horizontal, ram),
            1 => new NesMapperMmc1(prg, chr, ram),
            2 => new NesMapperUxrom(prg, chr, NesMirroring.Horizontal, false, ram),
            3 => new NesMapperCnrom(prg, chr, NesMirroring.Horizontal, ram),
            4 => new NesMapperMmc3(prg, chr, false, ram),
            5 => new NesMapperMmc5(prg, chr, NesMirroring.Horizontal, ram),
            7 => new NesMapperAxrom(prg, chr, ram),
            9 => new NesMapperMmc2(prg, chr, NesMirroring.Horizontal, ram),
            23 => new NesMapperVrc2Vrc4(prg, chr, NesMirroring.Horizontal, NesVrcRegisterMapping.Mapper23, ram),
            25 => new NesMapperVrc2Vrc4(prg, chr, NesMirroring.Horizontal, NesVrcRegisterMapping.Mapper25, ram),
            148 => new NesMapper148(prg, chr, NesMirroring.Horizontal, ram),
            _ => throw new AssertFailedException(),
        };

        if (mapper is NesMapperMmc5)
        {
            mapper.WriteExpansion(0x5102, 0x02);
            mapper.WriteExpansion(0x5103, 0x01);
        }

        mapper.WritePrgRam(0x6000, 0x5A);
        var save = new byte[mapper.BatterySaveSize];
        mapper.CopyBatterySave(save);

        Assert.AreEqual(0x2000, mapper.BatterySaveSize);
        Assert.AreEqual((byte)0x5A, save[0]);
        Assert.IsTrue(mapper.BatterySaveDirty);
    }

    private static byte[] Copy(NesBatteryRam ram)
    {
        var result = new byte[ram.Length];
        ram.CopyTo(result);
        return result;
    }

    private static byte[] CreateRom(int mapper, bool battery, byte prgRamUnits)
    {
        var rom = new byte[0x4010];
        rom[0] = (byte)'N';
        rom[1] = (byte)'E';
        rom[2] = (byte)'S';
        rom[3] = 0x1A;
        rom[4] = 1;
        rom[6] = (byte)((mapper << 4) | (battery ? 0x02 : 0));
        rom[7] = (byte)(mapper & 0xF0);
        rom[8] = prgRamUnits;
        return rom;
    }
}
