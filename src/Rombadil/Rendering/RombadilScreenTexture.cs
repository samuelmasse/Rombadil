namespace Rombadil;

[Rombadil]
public class RombadilScreenTexture(RombadilGl gl)
{
    private readonly Texture2D texture = new(gl, (NesPpu.ScreenWidth, NesPpu.ScreenHeight))
    {
        MinFilter = GlTextureMinFilter.Nearest,
        MagFilter = GlTextureMagFilter.Nearest,
        WrapS = GlTextureWrapMode.ClampToEdge,
        WrapT = GlTextureWrapMode.ClampToEdge
    };

    public Texture2D Texture => texture;

    public void Upload(ReadOnlySpan<byte> pixels) => texture.TexImage2D(pixels);
}
