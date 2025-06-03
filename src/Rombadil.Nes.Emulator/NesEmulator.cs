namespace Rombadil.Nes.Emulator;

public class NesEmulator
{
    private readonly CpuEmulatorState state;
    private readonly NesPpu ppu;
    private readonly NesController controller1;
    private readonly NesController controller2;
    private readonly NesMemoryBus bus;
    private readonly CpuEmulator6502 cpu;
    private readonly CpuEmulatorLogger logger;

    public NesEmulator(Memory<byte> rom, Memory<byte> framebuffer)
    {
        var romHeader = rom[..0x10];
        var header = new NesRomHeader(romHeader);

        var romPrg = rom.Slice(romHeader.Length, header.PrgRomSize * 0x4000);
        var romChr = rom.Slice(romHeader.Length + romPrg.Length, header.ChrRomSize * 0x2000);

        Memory<byte> chr = new byte[0x2000];

        var mapper = header.MapperNumber switch
        {
            0 => new NesMapperNrom(romPrg),
            1 => new NesMapperMmc1(),
            _ => new NesMapper()
        };

        romChr.CopyTo(chr);

        state = new CpuEmulatorState();
        ppu = new NesPpu(chr, framebuffer);
        controller1 = new NesController();
        controller2 = new NesController();
        bus = new NesMemoryBus(mapper, ppu, controller1, controller2);
        cpu = new CpuEmulator6502(state, bus);
        logger = new CpuEmulatorLogger(state, bus, cpu);

        Reset();
    }

    public void Reset()
    {
        cpu.Reset();
        ppu.Reset();
    }

    public void Step()
    {
        bool done = false;
        while (!done)
        {
            // Console.WriteLine(logger.Log());
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

    public void SetButtons1(NesButtons buttons) => controller1.SetButtons(buttons);
    public void SetButtons2(NesButtons buttons) => controller2.SetButtons(buttons);
}
