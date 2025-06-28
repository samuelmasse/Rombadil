using Rombadil;

byte[] rom;
try
{
    rom = File.ReadAllBytes(args[0]);
}
catch
{
    rom = new byte[0xFFFF];
    rom[4] = 1;
}

new RombadilLoop(rom).Run();
