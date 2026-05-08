using Rombadil;

// note : zelda upper door bug

string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string romFile = Path.Combine(documentsPath, "Roms", "NES", "Legend of Zelda, The (USA) (Rev 1).nes");

if (args.Length > 0)
    romFile = args[0];

var rom = File.ReadAllBytes(romFile);
new RombadilLoop(rom).Run();
