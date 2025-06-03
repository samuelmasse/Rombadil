namespace Rombadil;

public class NesEmulator
{
    private readonly CpuEmulatorState state;
    private readonly CpuEmulatorMemory memory;
    private readonly PpuNes ppu;
    private readonly NesController controller1;
    private readonly NesController controller2;
    private readonly NesMemoryBus bus;
    private readonly CpuEmulator6502 cpu;
    private readonly CpuEmulatorLogger logger;

    public NesEmulator(Memory<byte> rom, Pixels pixels)
    {
        var romHeader = rom[..0x10];
        var header = new NesRomHeader(romHeader);

        var romPrg = rom.Slice(romHeader.Length, header.PrgRomSize * 0x4000);
        var romChr = rom.Slice(romHeader.Length + romPrg.Length, header.ChrRomSize * 0x2000);

        Memory<byte> mem = new byte[0x10000];
        Memory<ushort> map = new ushort[0x10000];
        Memory<byte> chr = new byte[0x2000];
        for (int i = 0; i < map.Length; i++)
            map.Span[i] = (ushort)i;

        if (header.PrgRomSize == 1)
        {
            for (int i = 0; i < 0x4000; i++)
                map.Span[0xC000 + i] = (ushort)(0x8000 + i);
        }

        romPrg.CopyTo(mem.Slice(0x8000, 0x8000));
        romChr.CopyTo(chr);

        state = new CpuEmulatorState();
        memory = new CpuEmulatorMemory(mem, map);
        ppu = new PpuNes(chr, pixels);
        controller1 = new NesController();
        controller2 = new NesController();
        bus = new NesMemoryBus(memory, state, ppu, controller1, controller2);
        cpu = new CpuEmulator6502(state, memory, bus);
        logger = new CpuEmulatorLogger(state, memory, cpu);
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
