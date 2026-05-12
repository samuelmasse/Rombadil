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
        var header = new NesRomHeader(rom[..0x10]);
        int prgStart = 0x10 + (header.HasTrainer ? 0x200 : 0);

        var prg = rom.Slice(prgStart, header.PrgRomSize * 0x4000);
        var chr = rom.Slice(prgStart + prg.Length, header.ChrRomSize * 0x2000);

        var mirroring = header.FourScreen
            ? NesMirroring.FourScreen
            : header.VerticalMirroring ? NesMirroring.Vertical : NesMirroring.Horizontal;

        mapper = header.MapperNumber switch
        {
            0 => new NesMapperNrom(prg, chr, mirroring),
            1 => new NesMapperMmc1(prg, chr, header.PrgRamSize + header.PrgNvRamSize),
            2 => new NesMapperUxrom(prg, chr, mirroring, header.Submapper == 2),
            3 => new NesMapperCnrom(prg, chr, mirroring),
            4 => new NesMapperMmc3(prg, chr, header.FourScreen),
            5 => new NesMapperMmc5(prg, chr, mirroring),
            7 => new NesMapperAxrom(prg, chr),
            9 => new NesMapperMmc2(prg, chr, mirroring),
            23 => new NesMapperVrc2Vrc4(prg, chr, mirroring, GetMapper23VrcRegisterMapping(header)),
            25 => new NesMapperVrc2Vrc4(prg, chr, mirroring, GetMapper25VrcRegisterMapping(header)),
            148 => new NesMapper148(prg, chr, mirroring),
            _ => new NesMapper()
        };

        state = new CpuEmulatorState();
        ppu = new NesPpu(mapper, framebuffer);
        apu = new(mapper, samples);
        controller1 = new NesController();
        controller2 = new NesController();
        bus = new NesMemoryBus(state, mapper, ppu, apu, controller1, controller2);
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
            StepCpuInstruction();

        return state.Cycles - target;
    }

    public void StepFrame()
    {
        while (!StepCpuInstruction()) { }
    }

    private bool StepCpuInstruction()
    {
        bool frameCompleted = false;

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

        if (apu.PendingIrq)
            cpu.Irq();

        while (ppu.Cycles < state.Cycles * 3)
            frameCompleted |= ppu.Step();

        return frameCompleted;
    }

    public void SetButtons1(NesButtons buttons) => controller1.SetButtons(buttons);
    public void SetButtons2(NesButtons buttons) => controller2.SetButtons(buttons);

    private static NesVrcRegisterMapping GetMapper23VrcRegisterMapping(NesRomHeader header) => header.Submapper switch
    {
        1 or 3 => NesVrcRegisterMapping.Mapper23Vrc2BOrVrc4F,
        2 => NesVrcRegisterMapping.Mapper23Vrc4E,
        _ => NesVrcRegisterMapping.Mapper23,
    };

    private static NesVrcRegisterMapping GetMapper25VrcRegisterMapping(NesRomHeader header) => header.Submapper switch
    {
        1 or 3 => NesVrcRegisterMapping.Mapper25Vrc2COrVrc4B,
        2 => NesVrcRegisterMapping.Mapper25Vrc4D,
        _ => NesVrcRegisterMapping.Mapper25,
    };
}
