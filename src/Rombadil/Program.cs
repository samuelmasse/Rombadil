using Rombadil;

// note : zelda upper door bug

string romFile = @"C:\Users\Samuel\Desktop\NES\Super Mario Bros. (World).nes";
if (args.Length > 0)
    romFile = args[0];

using var window = new RombadilWindow();
var rom = File.ReadAllBytes(romFile);
var nes = new NesEmulator(rom, window.Framebuffer, window.Samples);
var sw = Stopwatch.StartNew();
bool paused = false;
double time = 0;
int speed = 60;
bool fullSpeed = false;

window.Render += (delta) =>
{
    var dt = sw.Elapsed.TotalSeconds;
    sw.Restart();

    time += dt * speed;
    int steps = (int)time;
    time -= steps;
    if (fullSpeed || steps > 16)
        steps = 1;

    if (window.IsKeyPressed(Keys.P)) fullSpeed = !fullSpeed;

    if (!fullSpeed)
    {
        if (speed > 15 && window.IsKeyPressed(Keys.LeftBracket)) speed /= 2;
        if (speed < 480 && window.IsKeyPressed(Keys.RightBracket)) speed *= 2;
    }

    if (window.IsKeyPressed(Keys.Escape)) paused = !paused;
    if (window.IsKeyDown(Keys.LeftControl) && window.IsKeyPressed(Keys.R)) nes.Reset();
    if (paused) return;

    NesButtons b1 = 0;
    if (window.IsKeyDown(Keys.S)) b1 |= NesButtons.A;
    if (window.IsKeyDown(Keys.A)) b1 |= NesButtons.B;
    if (window.IsKeyDown(Keys.W)) b1 |= NesButtons.Start;
    if (window.IsKeyDown(Keys.Q)) b1 |= NesButtons.Select;
    if (window.IsKeyDown(Keys.Up) && !window.IsKeyDown(Keys.Down)) b1 |= NesButtons.Up;
    if (window.IsKeyDown(Keys.Down) && !window.IsKeyDown(Keys.Up)) b1 |= NesButtons.Down;
    if (window.IsKeyDown(Keys.Left) && !window.IsKeyDown(Keys.Right)) b1 |= NesButtons.Left;
    if (window.IsKeyDown(Keys.Right) && !window.IsKeyDown(Keys.Left)) b1 |= NesButtons.Right;

    NesButtons b2 = 0;
    if (window.IsKeyDown(Keys.F)) b2 |= NesButtons.A;
    if (window.IsKeyDown(Keys.D)) b2 |= NesButtons.B;
    if (window.IsKeyDown(Keys.R)) b2 |= NesButtons.Start;
    if (window.IsKeyDown(Keys.E)) b2 |= NesButtons.Select;
    if (window.IsKeyDown(Keys.U) && !window.IsKeyDown(Keys.J)) b2 |= NesButtons.Up;
    if (window.IsKeyDown(Keys.J) && !window.IsKeyDown(Keys.U)) b2 |= NesButtons.Down;
    if (window.IsKeyDown(Keys.H) && !window.IsKeyDown(Keys.K)) b2 |= NesButtons.Left;
    if (window.IsKeyDown(Keys.K) && !window.IsKeyDown(Keys.H)) b2 |= NesButtons.Right;

    for (int i = 0; i < steps; i++)
    {
        nes.SetButtons1(b1);
        nes.SetButtons2(b2);
        nes.Step();

        while (window.Samples.Count > 0)
            window.AudioBuffer.Add(window.Samples.Dequeue());
        window.AudioBuffer.Submit(fullSpeed ? (1 / dt) : speed);
    }
};

window.Run();
