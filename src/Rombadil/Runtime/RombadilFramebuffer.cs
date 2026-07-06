namespace Rombadil;

[Rombadil]
public class RombadilFramebuffer
{
    private readonly byte[] pixels = new byte[NesPpu.ScreenWidth * NesPpu.ScreenHeight * 4];

    public byte[] Pixels => pixels;
}
