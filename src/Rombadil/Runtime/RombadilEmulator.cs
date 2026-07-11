namespace Rombadil;

[Rombadil]
public class RombadilEmulator(RombadilRom rom, RombadilFramebuffer framebuffer, RombadilAudio audio)
{
    private readonly NesEmulator nes = new(rom.Bytes, framebuffer.Pixels, audio.Samples);

    public void Reset() => nes.Reset();

    public long Step(long cycles) => nes.Step(cycles);

    public void SetButtons1(NesButtons buttons) => nes.SetButtons1(buttons);

    public void SetButtons2(NesButtons buttons) => nes.SetButtons2(buttons);

    public int BatterySaveSize => nes.BatterySaveSize;

    public bool BatterySaveDirty => nes.BatterySaveDirty;

    public void LoadBatterySave(ReadOnlySpan<byte> source) => nes.LoadBatterySave(source);

    public void CopyBatterySave(Span<byte> destination) => nes.CopyBatterySave(destination);

    public void MarkBatterySaveClean() => nes.MarkBatterySaveClean();
}
