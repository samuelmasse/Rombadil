using Rombadil;

string romFile = @"C:\Users\Samuel\Desktop\NES\Super Mario Bros. 3 (USA) (Rev A).nes";
if (args.Length > 0)
    romFile = args[0];

using var window = new RombadilWindow();
var rom = File.ReadAllBytes(romFile);
var nes = new NesEmulator(rom, window.Framebuffer);
var sw = Stopwatch.StartNew();

window.Render += (delta) =>
{
    sw.Restart();
    NesButtons b = 0;

    if (window.IsKeyDown(Keys.LeftControl) && window.IsKeyPressed(Keys.R)) nes.Reset();
    if (window.IsKeyDown(Keys.S)) b |= NesButtons.A;
    if (window.IsKeyDown(Keys.A)) b |= NesButtons.B;
    if (window.IsKeyDown(Keys.W)) b |= NesButtons.Start;
    if (window.IsKeyDown(Keys.Q)) b |= NesButtons.Select;
    if (window.IsKeyDown(Keys.Up)) b |= NesButtons.Up;
    if (window.IsKeyDown(Keys.Down)) b |= NesButtons.Down;
    if (window.IsKeyDown(Keys.Left)) b |= NesButtons.Left;
    if (window.IsKeyDown(Keys.Right)) b |= NesButtons.Right;

    nes.SetButtons1(b);
    nes.Step();

    Console.WriteLine($"time {sw.Elapsed.TotalMilliseconds}");
};

window.Run();
