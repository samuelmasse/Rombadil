namespace Rombadil;

[Rombadil]
public class RombadilRunState(
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootGamepads gamepads,
    RootInput input,
    RootKeyboard keyboard,
    RootMouse mouse,
    RootSprites sprites,
    RombadilScope scope,
    RombadilAudio audio,
    RombadilEmulator emulator,
    RombadilFramebuffer framebuffer,
    RombadilScreenTexture screenTexture) : State
{
    private const double CursorHideDelay = 1.0;

    private readonly Stopwatch sw = new();

    private double cycleAccumulator;
    private double speed = 1.0;
    private double smoothedEffectiveSpeed;
    private bool paused;
    private bool fullSpeed;
    private double cursorIdleSeconds;
    private Vec2 cursorPosition;
    private bool cursorHidden;

    public override void Unload() =>
        scope.Scope<RombadilLoaderScope>()
            .Run(x => x.Get<RombadilRuntimeUnloader>().Run());

    public override void Update(double delta)
    {
        UpdateCursor(delta);

        long cycles = TimeStep(out double effectiveSpeed);

        EmulatorInput();

        var c1 = FilterInput(KeyboardInput(Keys.S, Keys.A, Keys.W, Keys.Q,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right) | ControllerInput(0));

        var c2 = FilterInput(KeyboardInput(Keys.F, Keys.D, Keys.R, Keys.E,
            Keys.U, Keys.J, Keys.H, Keys.K) | ControllerInput(1));

        if (!paused && cycles > 0)
        {
            emulator.SetButtons1(c1);
            emulator.SetButtons2(c2);

            long overshoot = emulator.Step(cycles);
            cycleAccumulator -= overshoot;
        }

        audio.Pump(effectiveSpeed);
    }

    public override void Render()
    {
        backbuffer.Clear();
        screenTexture.Upload(framebuffer.Pixels);
    }

    public override void Draw()
    {
        Vec2 size = canvas.Size;
        if (size.X <= 0 || size.Y <= 0)
            return;

        float targetAspect = (NesPpu.ScreenWidth * 8f / 7f) / NesPpu.ScreenHeight;
        float windowAspect = size.X / size.Y;

        Vec2 drawSize = windowAspect > targetAspect
            ? new(size.Y * targetAspect, size.Y)
            : new(size.X, size.X / targetAspect);

        var texture = screenTexture.Texture;
        sprites.Batch.Draw(
            texture,
            (size - drawSize) / 2,
            drawSize,
            Vec2.Zero,
            texture.Size,
            (1f, 1f, 1f, 1f),
            SpriteBatchRotation.None,
            SpriteBatchFlip.Vertical);
    }

    private long TimeStep(out double effectiveSpeed)
    {
        var dt = sw.Elapsed.TotalSeconds;
        sw.Restart();

        if (fullSpeed)
        {
            cycleAccumulator = 0;
            double instant = dt > 0 ? RombadilNesTiming.CyclesPerFrame / (dt * RombadilNesTiming.CpuHz) : 1.0;
            smoothedEffectiveSpeed = 0.95 * smoothedEffectiveSpeed + 0.05 * instant;
            effectiveSpeed = smoothedEffectiveSpeed;
            return RombadilNesTiming.CyclesPerFrame;
        }

        cycleAccumulator += dt * RombadilNesTiming.CpuHz * speed;

        double maxCycles = RombadilNesTiming.CyclesPerFrame * 2 * speed;
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
        if (keyboard.IsKeyPressed(Keys.P))
            fullSpeed = !fullSpeed;

        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressed(Keys.R))
        {
            emulator.Reset();
            audio.Drop();
        }

        if (!fullSpeed)
        {
            if (speed > 0.25 && keyboard.IsKeyPressed(Keys.LeftBracket)) speed /= 2;
            if (speed < 8.0 && keyboard.IsKeyPressed(Keys.RightBracket)) speed *= 2;
        }
    }

    private NesButtons KeyboardInput(Keys a, Keys b, Keys start, Keys select, Keys up, Keys down, Keys left, Keys right)
    {
        NesButtons c = 0;

        if (keyboard.IsKeyDown(a)) c |= NesButtons.A;
        if (keyboard.IsKeyDown(b)) c |= NesButtons.B;
        if (keyboard.IsKeyDown(start)) c |= NesButtons.Start;
        if (keyboard.IsKeyDown(select)) c |= NesButtons.Select;
        if (keyboard.IsKeyDown(up)) c |= NesButtons.Up;
        if (keyboard.IsKeyDown(down)) c |= NesButtons.Down;
        if (keyboard.IsKeyDown(left)) c |= NesButtons.Left;
        if (keyboard.IsKeyDown(right)) c |= NesButtons.Right;

        return c;
    }

    private NesButtons ControllerInput(int index)
    {
        NesButtons c = 0;

        if (gamepads.IsButtonDown(index, GamepadButtons.A)) c |= NesButtons.A;
        if (gamepads.IsButtonDown(index, GamepadButtons.X)) c |= NesButtons.B;
        if (gamepads.IsButtonDown(index, GamepadButtons.Start)) c |= NesButtons.Start;
        if (gamepads.IsButtonDown(index, GamepadButtons.Back)) c |= NesButtons.Select;
        if (gamepads.IsButtonDown(index, GamepadButtons.DPadUp)) c |= NesButtons.Up;
        if (gamepads.IsButtonDown(index, GamepadButtons.DPadDown)) c |= NesButtons.Down;
        if (gamepads.IsButtonDown(index, GamepadButtons.DPadLeft)) c |= NesButtons.Left;
        if (gamepads.IsButtonDown(index, GamepadButtons.DPadRight)) c |= NesButtons.Right;

        if (gamepads.Axis(index, GamepadAxis.LeftX) < -0.4) c |= NesButtons.Left;
        if (gamepads.Axis(index, GamepadAxis.LeftX) > 0.4) c |= NesButtons.Right;

        if (gamepads.Axis(index, GamepadAxis.LeftY) < -0.4) c |= NesButtons.Up;
        if (gamepads.Axis(index, GamepadAxis.LeftY) > 0.4) c |= NesButtons.Down;

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

    private void UpdateCursor(double delta)
    {
        if (mouse.Position != cursorPosition)
        {
            cursorPosition = mouse.Position;
            cursorIdleSeconds = 0;
            ShowCursor();
            return;
        }

        cursorIdleSeconds += delta;
        if (cursorIdleSeconds >= CursorHideDelay)
            HideCursor();
    }

    private void ShowCursor()
    {
        if (!cursorHidden)
            return;

        cursorHidden = false;
        input.CursorMode = CursorMode.Normal;
    }

    private void HideCursor()
    {
        if (cursorHidden)
            return;

        cursorHidden = true;
        input.CursorMode = CursorMode.Hidden;
    }
}
