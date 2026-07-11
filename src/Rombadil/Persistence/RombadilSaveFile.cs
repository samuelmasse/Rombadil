namespace Rombadil;

public sealed class RombadilSaveFile(RombadilRom rom, string root)
{
    private readonly string path = Path.Combine(root, $"{Convert.ToHexString(SHA256.HashData(rom.Bytes)).ToLowerInvariant()}.sav");

    public static string DefaultRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "My Games",
        "Rombadil",
        "Saves");

    public byte[]? Load(int expectedLength)
    {
        if (!File.Exists(path))
            return null;

        byte[] data = File.ReadAllBytes(path);
        if (data.Length != expectedLength)
            throw new InvalidDataException($"Battery save has {data.Length} bytes; expected {expectedLength}.");

        return data;
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        Directory.CreateDirectory(root);
        string tempPath = path + ".tmp";

        using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(data);
            stream.Flush(flushToDisk: true);
        }

        File.Move(tempPath, path, overwrite: true);
    }
}
