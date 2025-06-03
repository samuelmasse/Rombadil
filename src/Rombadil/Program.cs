using Rombadil;

var pixels = new Pixels((256, 240));
using var canvas = new Canvas(pixels);

string romFile = @"C:\Users\Samuel\Desktop\NES\Super Mario Bros. (World).nes";
if (args.Length > 0)
    romFile = args[0];

var rom = File.ReadAllBytes(romFile);
var sw = Stopwatch.StartNew();
var nes = new NesEmulator(rom, pixels);
nes.Reset();

canvas.Render += (delta) =>
{
    sw.Restart();
    NesButtons b = 0;

    if (canvas.IsKeyDown(Keys.LeftControl) && canvas.IsKeyPressed(Keys.R)) nes.Reset();
    if (canvas.IsKeyDown(Keys.S)) b |= NesButtons.A;
    if (canvas.IsKeyDown(Keys.A)) b |= NesButtons.B;
    if (canvas.IsKeyDown(Keys.W)) b |= NesButtons.Start;
    if (canvas.IsKeyDown(Keys.Q)) b |= NesButtons.Select;
    if (canvas.IsKeyDown(Keys.Up)) b |= NesButtons.Up;
    if (canvas.IsKeyDown(Keys.Down)) b |= NesButtons.Down;
    if (canvas.IsKeyDown(Keys.Left)) b |= NesButtons.Left;
    if (canvas.IsKeyDown(Keys.Right)) b |= NesButtons.Right;

    nes.SetButtons1(b);
    nes.Step();

    Console.WriteLine($"time {sw.Elapsed.TotalMilliseconds}");
};

canvas.Run();
