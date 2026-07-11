namespace Rombadil;

[RombadilLoader]
public class RombadilRuntimeUnloader(RombadilAudio audio, RombadilBatterySave batterySave, RombadilGl gl)
{
    public void Run()
    {
        batterySave.Close();
        audio.Dispose();
        gl.Dispose();
    }
}
