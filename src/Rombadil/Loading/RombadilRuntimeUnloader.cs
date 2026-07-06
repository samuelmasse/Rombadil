namespace Rombadil;

[RombadilLoader]
public class RombadilRuntimeUnloader(RombadilAudio audio, RombadilGl gl)
{
    public void Run()
    {
        audio.Dispose();
        gl.Dispose();
    }
}
