namespace Rombadil;

[RombadilLoader]
public class RombadilRuntimeLoader(RombadilAudio audio, RombadilBatterySave batterySave)
{
    public void Run()
    {
        batterySave.Load();
        audio.Start();
    }
}
