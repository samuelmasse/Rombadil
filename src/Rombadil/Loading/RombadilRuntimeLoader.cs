namespace Rombadil;

[RombadilLoader]
public class RombadilRuntimeLoader(RombadilAudio audio)
{
    public void Run() => audio.Start();
}
