namespace Rombadil.Nes.Emulator.Test;

[TestClass]
public class PpuFullPaletteVisualTest
{
    private const int TargetFrames = 10;

    [TestMethod]
    public void FullPalette_MatchesKnownGoodScreenshot()
    {
        byte[] rom = File.ReadAllBytes(Path.Join("full_palette", "full_palette.nes"));
        byte[] framebuffer = new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 3];
        var samples = new List<int>();
        var emulator = new NesEmulator(rom, framebuffer, samples);

        for (int frame = 0; frame < TargetFrames; frame++)
        {
            emulator.StepFrame();
            samples.Clear();
        }

        using var expectedStream = File.OpenRead(Path.Join("full_palette", "full_palette.png"));
        var expected = Png.Open(expectedStream);

        Assert.AreEqual(NesPpu.ScreenWidth, expected.Width);
        Assert.AreEqual(NesPpu.ScreenHeight, expected.Height);

        for (int y = 0; y < NesPpu.ScreenHeight; y++)
        {
            for (int x = 0; x < NesPpu.ScreenWidth; x++)
            {
                var pixel = expected.GetPixel(x, y);
                int i = (y * NesPpu.ScreenWidth + x) * 3;

                if (framebuffer[i] != pixel.R ||
                    framebuffer[i + 1] != pixel.G ||
                    framebuffer[i + 2] != pixel.B)
                {
                    Assert.Fail(
                        $"Pixel mismatch at ({x}, {y}) after {TargetFrames} frames. " +
                        $"Expected RGB({pixel.R}, {pixel.G}, {pixel.B}), " +
                        $"actual RGB({framebuffer[i]}, {framebuffer[i + 1]}, {framebuffer[i + 2]}).");
                }
            }
        }
    }
}
