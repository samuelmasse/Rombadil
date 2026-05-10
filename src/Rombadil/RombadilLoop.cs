namespace Rombadil;

public class RombadilLoop
{
    private static readonly double CpuHz = 1789773;
    private static readonly long CyclesPerFrame = (long)Math.Ceiling(CpuHz / 60);

    private readonly RombadilWindow window;
    private readonly RombadilAudio audio;
    private readonly NesEmulator nes;
    private readonly Stopwatch sw;
    private double cycleAccumulator;
    private double speed;
    private double smoothedEffectiveSpeed;
    private bool paused;
    private bool fullSpeed;

    public RombadilLoop(byte[] rom)
    {
        window = new();
        audio = new RombadilAudio(CpuHz);
        nes = new(rom, window.Framebuffer, audio.Samples);
        sw = new();

        speed = 1.0;
        window.Render += Render;
        window.Load += audio.Start;
        window.Unload += audio.Dispose;
    }

    private void Render(double obj)
    {
        long cycles = TimeStep(out double effectiveSpeed);

        EmulatorInput();

        var c1 = FilterInput(KeyboardInput(Keys.S, Keys.A, Keys.W, Keys.Q,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right) | ControllerInput(0));

        var c2 = FilterInput(KeyboardInput(Keys.F, Keys.D, Keys.R, Keys.E,
            Keys.U, Keys.J, Keys.H, Keys.K) | ControllerInput(1));

        if (!paused && cycles > 0)
        {
            nes.SetButtons1(c1);
            nes.SetButtons2(c2);

            long overshoot = nes.Step(cycles);
            cycleAccumulator -= overshoot;
        }

        audio.Pump(effectiveSpeed);
    }

    private long TimeStep(out double effectiveSpeed)
    {
        var dt = sw.Elapsed.TotalSeconds;
        sw.Restart();

        if (fullSpeed)
        {
            cycleAccumulator = 0;
            double instant = dt > 0 ? CyclesPerFrame / (dt * CpuHz) : 1.0;
            smoothedEffectiveSpeed = 0.95 * smoothedEffectiveSpeed + 0.05 * instant;
            effectiveSpeed = smoothedEffectiveSpeed;
            return CyclesPerFrame;
        }

        cycleAccumulator += dt * CpuHz * speed;

        double maxCycles = CyclesPerFrame * 2 * speed;
        if (cycleAccumulator > maxCycles)
            cycleAccumulator = maxCycles;

        long cycles = (long)cycleAccumulator;
        cycleAccumulator -= cycles;
        smoothedEffectiveSpeed = speed;
        effectiveSpeed = speed;
        return cycles;
    }

    private void EmulatorInput()
    {
        if (window.IsKeyPressed(Keys.P))
            fullSpeed = !fullSpeed;

        if (window.IsKeyPressed(Keys.Escape))
            paused = !paused;

        if (window.IsKeyDown(Keys.LeftControl) && window.IsKeyPressed(Keys.R))
        {
            nes.Reset();
            audio.Drop();
        }

        if (!fullSpeed)
        {
            if (speed > 0.25 && window.IsKeyPressed(Keys.LeftBracket)) speed /= 2;
            if (speed < 8.0 && window.IsKeyPressed(Keys.RightBracket)) speed *= 2;
        }
    }

    private NesButtons KeyboardInput(Keys a, Keys b, Keys start, Keys select, Keys up, Keys down, Keys left, Keys right)
    {
        NesButtons c = 0;

        if (window.IsKeyDown(a)) c |= NesButtons.A;
        if (window.IsKeyDown(b)) c |= NesButtons.B;
        if (window.IsKeyDown(start)) c |= NesButtons.Start;
        if (window.IsKeyDown(select)) c |= NesButtons.Select;
        if (window.IsKeyDown(up)) c |= NesButtons.Up;
        if (window.IsKeyDown(down)) c |= NesButtons.Down;
        if (window.IsKeyDown(left)) c |= NesButtons.Left;
        if (window.IsKeyDown(right)) c |= NesButtons.Right;

        return c;
    }

    private NesButtons ControllerInput(int index)
    {
        NesButtons c = 0;

        if (window.IsControllerButtonDown(index, ControllerButtons.A)) c |= NesButtons.A;
        if (window.IsControllerButtonDown(index, ControllerButtons.X)) c |= NesButtons.B;
        if (window.IsControllerButtonDown(index, ControllerButtons.Start)) c |= NesButtons.Start;
        if (window.IsControllerButtonDown(index, ControllerButtons.Select)) c |= NesButtons.Select;
        if (window.IsControllerButtonDown(index, ControllerButtons.DpadUp)) c |= NesButtons.Up;
        if (window.IsControllerButtonDown(index, ControllerButtons.DpadDown)) c |= NesButtons.Down;
        if (window.IsControllerButtonDown(index, ControllerButtons.DpadLeft)) c |= NesButtons.Left;
        if (window.IsControllerButtonDown(index, ControllerButtons.DpadRight)) c |= NesButtons.Right;

        if (window.GetAxis(index, ControllerAxis.LeftStickX) < -0.4) c |= NesButtons.Left;
        if (window.GetAxis(index, ControllerAxis.LeftStickX) > 0.4) c |= NesButtons.Right;

        if (window.GetAxis(index, ControllerAxis.LeftStickY) < -0.4) c |= NesButtons.Up;
        if (window.GetAxis(index, ControllerAxis.LeftStickY) > 0.4) c |= NesButtons.Down;

        return c;
    }

    private NesButtons FilterInput(NesButtons buttons)
    {
        if ((buttons & NesButtons.Left) != 0 && (buttons & NesButtons.Right) != 0)
        {
            buttons &= ~NesButtons.Left;
            buttons &= ~NesButtons.Right;
        }

        if ((buttons & NesButtons.Up) != 0 && (buttons & NesButtons.Down) != 0)
        {
            buttons &= ~NesButtons.Up;
            buttons &= ~NesButtons.Down;
        }

        return buttons;
    }

    public void Run()
    {
        window.Run();
        window.Dispose();
    }
}
