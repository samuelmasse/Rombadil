Dir("tests", "Rombadil.Nes.Emulator.Test", out var testDir);

CaptureForTest("full_palette", "full_palette", 10);

void CaptureForTest(string dir, string file, int frames) =>
    Capture(Absolute(Path.Join(testDir, dir, file + ".nes")), frames,
    Absolute(Path.Join(testDir, dir, file + ".png")));

static void Capture(string romPath, int frames, string outputPath)
{
    byte[] rom = File.ReadAllBytes(romPath);
    byte[] framebuffer = new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 3];
    var samples = new List<int>();
    var emulator = new NesEmulator(rom, framebuffer, samples);

    for (int frame = 0; frame < frames; frame++)
    {
        emulator.StepFrame();
        samples.Clear();
    }

    var png = PngBuilder.Create(NesPpu.ScreenWidth, NesPpu.ScreenHeight, hasAlphaChannel: false);
    for (int y = 0; y < NesPpu.ScreenHeight; y++)
    {
        for (int x = 0; x < NesPpu.ScreenWidth; x++)
        {
            int i = (y * NesPpu.ScreenWidth + x) * 3;
            png.SetPixel(new Pixel(framebuffer[i], framebuffer[i + 1], framebuffer[i + 2]), x, y);
        }
    }

    string? outputDir = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(outputDir))
        Directory.CreateDirectory(outputDir);

    using var output = File.Create(outputPath);
    png.Save(output);
}
