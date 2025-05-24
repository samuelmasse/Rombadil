namespace Rombadil.Assembler.Test;

[TestClass]
public sealed class Assembler6502Test
{
    [TestMethod]
    public void Assemble_ImmediateLoadA_CorrectBytes()
    {
        string[] lines = ["LDA #$10"];

        var binary = new Assembler6502().Assemble(lines);

        Assert.AreEqual(2, binary.Length);
        Assert.AreEqual(0xA9, binary[0]);
        Assert.AreEqual(0x10, binary[1]);
    }
}
