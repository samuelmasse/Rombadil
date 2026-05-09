namespace Rombadil.Nes.Emulator;

public class NesEmulator
{
    private readonly CpuEmulatorState state;
    private readonly NesMapper mapper;
    private readonly NesPpu ppu;
    private readonly NesApu apu;
    private readonly NesController controller1;
    private readonly NesController controller2;
    private readonly NesMemoryBus bus;
    private readonly CpuEmulator6502 cpu;
    private readonly CpuEmulatorLogger logger;

    public NesEmulator(Memory<byte> rom, Memory<byte> framebuffer, List<int> samples)
    {
        var romHeader = rom[..0x10];
        var header = new NesRomHeader(rom[..0x10]);

        var prg = rom.Slice(romHeader.Length, header.PrgRomSize * 0x4000);
        var chr = rom.Slice(romHeader.Length + prg.Length, header.ChrRomSize * 0x2000);

        mapper = header.MapperNumber switch
        {
            0 => new NesMapperNrom(prg, chr),
            1 => new NesMapperMmc1(prg, chr),
            2 => new NesMapperUxrom(prg, chr),
            4 => new NesMapperMmc3(prg, chr),
            _ => new NesMapper()
        };

        state = new CpuEmulatorState();
        ppu = new NesPpu(mapper, framebuffer);
        apu = new(mapper, samples);
        controller1 = new NesController();
        controller2 = new NesController();
        bus = new NesMemoryBus(mapper, ppu, apu, controller1, controller2);
        cpu = new CpuEmulator6502(state, bus);
        logger = new CpuEmulatorLogger(state, bus, cpu);

        Reset();
    }

    public void Reset()
    {
        cpu.Reset();
        ppu.Reset();
        apu.Reset();
    }

    public long Step(long cycles)
    {
        long target = state.Cycles + cycles;

        while (state.Cycles < target)
        {
            cpu.Step();

            while (apu.Cycles < state.Cycles)
                apu.Step();

            if (ppu.PendingNmi)
            {
                cpu.Nmi();
                ppu.ClearPendingNmi();
            }

            if (mapper.PendingIrq)
            {
                cpu.Irq();
                mapper.ClearPendingIrq();
            }

            while (ppu.Cycles < state.Cycles * 3)
                ppu.Step();
        }

        return state.Cycles - target;
    }

    public void SetButtons1(NesButtons buttons) => controller1.SetButtons(buttons);
    public void SetButtons2(NesButtons buttons) => controller2.SetButtons(buttons);
}
