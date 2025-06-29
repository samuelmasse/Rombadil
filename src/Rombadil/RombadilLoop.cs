namespace Rombadil;

public class RombadilLoop
{
    private readonly RombadilWindow window;
    private readonly NesEmulator nes;
    private readonly Stopwatch sw;
    private double time;
    private int speed;
    private bool paused;
    private bool fullSpeed;

    public RombadilLoop(byte[] rom)
    {
        window = new();
        nes = new(rom, window.Framebuffer, window.Samples);
        sw = new();

        speed = 60;
        window.Render += Render;
    }

    private void Render(double obj)
    {
        var (rate, steps) = TimeStep();

        EmulatorInput();

        var c1 = FilterInput(KeyboardInput(Keys.S, Keys.A, Keys.W, Keys.Q,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right) | ControllerInput(0));

        var c2 = FilterInput(KeyboardInput(Keys.F, Keys.D, Keys.R, Keys.E,
            Keys.U, Keys.J, Keys.H, Keys.K) | ControllerInput(1));

        while (steps > 0 && !paused)
        {
            nes.SetButtons1(c1);
            nes.SetButtons2(c2);

            nes.Step();
            window.Step(rate);

            steps--;
        }
    }

    private (double, int) TimeStep()
    {
        var dt = sw.Elapsed.TotalSeconds;
        sw.Restart();

        time += dt * speed;
        int steps = (int)time;
        time -= steps;

        if (fullSpeed || steps > 16)
            steps = 1;

        var rate = fullSpeed ? (1 / dt) : speed;
        return (rate, steps);
    }

    private void EmulatorInput()
    {
        if (window.IsKeyPressed(Keys.P)) fullSpeed = !fullSpeed;
        if (window.IsKeyPressed(Keys.Escape)) paused = !paused;
        if (window.IsKeyDown(Keys.LeftControl) && window.IsKeyPressed(Keys.R)) nes.Reset();

        if (!fullSpeed)
        {
            if (speed > 15 && window.IsKeyPressed(Keys.LeftBracket)) speed /= 2;
            if (speed < 480 && window.IsKeyPressed(Keys.RightBracket)) speed *= 2;
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
