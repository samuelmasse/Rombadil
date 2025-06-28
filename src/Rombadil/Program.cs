using Rombadil;

var rom = File.ReadAllBytes(args[0]);
new RombadilLoop(rom).Run();
