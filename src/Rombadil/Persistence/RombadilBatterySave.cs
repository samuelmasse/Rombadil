namespace Rombadil;

[Rombadil]
public sealed class RombadilBatterySave(RombadilRom rom, RombadilEmulator emulator)
{
    private const double FlushIntervalSeconds = 2;

    private readonly byte[] snapshot = new byte[emulator.BatterySaveSize];
    private readonly RombadilSaveFile file = new(rom, RombadilSaveFile.DefaultRoot());
    private double flushElapsed;
    private bool loaded;

    public void Load()
    {
        if (snapshot.Length == 0 || loaded)
            return;

        byte[]? data = file.Load(snapshot.Length);
        if (data != null)
            emulator.LoadBatterySave(data);

        loaded = true;
    }

    public void Update(double delta)
    {
        if (!loaded)
            return;

        flushElapsed += delta;
        if (flushElapsed < FlushIntervalSeconds)
            return;

        flushElapsed %= FlushIntervalSeconds;
        Flush();
    }

    public void Flush()
    {
        if (!loaded || !emulator.BatterySaveDirty)
            return;

        emulator.CopyBatterySave(snapshot);
        file.Write(snapshot);
        emulator.MarkBatterySaveClean();
    }

    public void Close()
    {
        if (!loaded)
            return;

        Flush();
        loaded = false;
    }
}
