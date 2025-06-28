using Rombadil;

// note : zelda upper door bug

string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
string romFile = Path.Combine(desktopPath, "NES", "Super Mario Bros. (World).nes");

if (args.Length > 0)
    romFile = args[0];

var rom = File.ReadAllBytes(romFile);
new RombadilLoop(rom).Run();
