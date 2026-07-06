namespace Rombadil;

[Rombadil]
public class RombadilEmulator(RombadilRom rom, RombadilFramebuffer framebuffer, RombadilAudio audio)
{
    private readonly NesEmulator nes = new(rom.Bytes, framebuffer.Pixels, audio.Samples);

    public void Reset() => nes.Reset();

    public long Step(long cycles) => nes.Step(cycles);

    public void SetButtons1(NesButtons buttons) => nes.SetButtons1(buttons);

    public void SetButtons2(NesButtons buttons) => nes.SetButtons2(buttons);
}
