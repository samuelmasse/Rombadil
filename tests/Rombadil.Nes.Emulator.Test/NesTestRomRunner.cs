namespace Rombadil.Nes.Emulator.Test;

internal sealed class NesTestRomRunner
{
    private readonly List<int> samples = [];
    private readonly NesEmulator emulator;

    private NesTestRomRunner(byte[] rom)
    {
        byte[] framebuffer = new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 4];
        emulator = new NesEmulator(rom, framebuffer, samples);
    }

    public static NesTestRomRunner Load(string directory, string name)
    {
        byte[] rom = File.ReadAllBytes(Path.Join(directory, $"{name}.nes"));
        return new NesTestRomRunner(rom);
    }

    public byte Peek(ushort addr) => emulator.PeekCpuMemory(addr);

    public void StepFrame()
    {
        emulator.StepFrame();
        samples.Clear();
    }

    public void RunUntil(Func<NesTestRomRunner, bool> completed, int maxFrames, string timeoutMessage)
    {
        for (int frame = 0; frame < maxFrames; frame++)
        {
            StepFrame();

            if (completed(this))
                return;
        }

        Assert.Fail(timeoutMessage);
    }

    public string ReadAscii(ushort addr, int maxLength)
    {
        List<byte> bytes = [];
        for (int i = 0; i < maxLength; i++)
        {
            byte value = Peek((ushort)(addr + i));
            if (value == 0)
                break;

            bytes.Add(value);
        }

        return Encoding.ASCII.GetString([.. bytes]);
    }
}
