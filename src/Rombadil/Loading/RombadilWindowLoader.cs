namespace Rombadil;

[RombadilLoader]
public class RombadilWindowLoader(RootScreen screen)
{
    private const float NtscPixelAspect = 8f / 7f;

    public void Run()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        float correctedPixelWidth = NesPpu.ScreenWidth * NtscPixelAspect;
        var monitorSize = screen.MonitorSize;
        float scale = MathF.Min(
            monitorSize.X * 4 / 5 / correctedPixelWidth,
            monitorSize.Y * 4 / 5 / (float)NesPpu.ScreenHeight);

        screen.Title = "Rombadil";
        screen.Size = ((uint)Math.Round(correctedPixelWidth * scale), (uint)Math.Round(NesPpu.ScreenHeight * scale));
        SetIcon();
        screen.IsVisible = true;
    }

    private void SetIcon()
    {
        using var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rombadil.Icon.png");
        if (iconStream == null)
            return;

        var icon = Png.Open(iconStream);
        var pixels = new Vec4u8[icon.Width * icon.Height];

        for (int y = 0; y < icon.Height; y++)
        {
            for (int x = 0; x < icon.Width; x++)
            {
                var pixel = icon.GetPixel(x, y);
                int index = y * icon.Width + x;
                pixels[index] = (pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        screen.SetIcon(((uint)icon.Width, (uint)icon.Height), pixels);
    }
}
