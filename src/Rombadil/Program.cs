using Rombadil;

RootLoop.RunGlfw<RootRombadilLoadState>(injector =>
{
    byte[] rom;
    try
    {
        rom = File.ReadAllBytes(args[0]);
    }
    catch
    {
        rom = new byte[0xFFFF];
        rom[4] = 1;
    }

    injector.Add(new RombadilStartupRom(rom));
});
