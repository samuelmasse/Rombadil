namespace Rombadil.Test;

[TestClass]
public sealed class RombadilSaveFileTest
{
    private string root = null!;
    private RombadilRom rom = null!;

    [TestInitialize]
    public void Initialize()
    {
        root = Path.Combine(Path.GetTempPath(), "Rombadil.Test", Guid.NewGuid().ToString("N"));
        rom = new RombadilRom([1, 2, 3, 4]);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    [TestMethod]
    public void DefaultRoot_IsDocumentsMyGamesRombadilSaves()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Assert.AreEqual(Path.Combine(documents, "My Games", "Rombadil", "Saves"), RombadilSaveFile.DefaultRoot());
    }

    [TestMethod]
    public void MissingSave_ReturnsNullWithoutCreatingDirectory()
    {
        var file = new RombadilSaveFile(rom, root);

        Assert.IsNull(file.Load(4));
        Assert.IsFalse(Directory.Exists(root));
    }

    [TestMethod]
    public void Write_ReplacesSaveForSameRomContent()
    {
        var file = new RombadilSaveFile(rom, root);
        file.Write([1, 1]);
        file.Write([2, 2]);

        var sameRom = new RombadilSaveFile(new RombadilRom([1, 2, 3, 4]), root);

        CollectionAssert.AreEqual(new byte[] { 2, 2 }, sameRom.Load(2));
        Assert.AreEqual(1, Directory.GetFiles(root, "*.sav").Length);
        Assert.AreEqual(0, Directory.GetFiles(root, "*.tmp").Length);
    }

    [TestMethod]
    public void Load_RejectsUnexpectedLength()
    {
        var file = new RombadilSaveFile(rom, root);
        file.Write([1]);

        Assert.ThrowsExactly<InvalidDataException>(() => file.Load(2));
    }
}
