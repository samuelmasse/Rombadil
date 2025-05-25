namespace Rombadil.Assembler.Test;

[TestClass]
public class Assembler6502ErrorTest
{
    [TestMethod]
    public void Assemble_SelfReference_Throws()
    {
        string[] lines = ["VAR1 = $00 + VAR2", "VAR2 = VAR1 + 2"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Could not evaluate expression \"$00+VAR2\" for constant \"VAR1\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameStartsWithNumber_Throws()
    {
        string[] lines = ["1VAR = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Name \"1VAR\" must begin with a letter.", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameEmpty_Throws()
    {
        string[] lines = [" = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Name cannot be empty.", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameInvalidChar_Throws()
    {
        string[] lines = ["a_%$@im = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Name \"a_%$@im\" contains an invalid character '%'. Only letters, digits, and underscores are allowed.", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadInstruction_Throws()
    {
        string[] lines = ["bad $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Unrecognized instruction \"bad\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeSingleByte_Throws()
    {
        string[] lines = ["lda #257"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand value 257 is out of range for instruction \"LDA\". Expected 8-bit value (0 to 255).", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeSingleByte_Throws()
    {
        string[] lines = ["lda -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand value -1 is out of range for instruction \"LDA\". Expected 8-bit value (0 to 255).", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeTwoByte_Throws()
    {
        string[] lines = ["sta $FFFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand value 1048575 is out of range for instruction \"STA\". Expected 16-bit value (0 to 65535).", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeTwoByte_Throws()
    {
        string[] lines = ["sta -$FFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand value -4095 is out of range for instruction \"STA\". Expected 16-bit value (0 to 65535).", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadAddressingMode_Throws()
    {
        string[] lines = ["lda ($00)"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("No opcode exists for instruction \"LDA\" with addressing mode \"Indirect\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_InvalidAddressingModeFormat_Throws()
    {
        string[] lines = ["lda ($00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Unable to resolve operand value \"($00\" for instruction \"LDA\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadDirective_Throws()
    {
        string[] lines = [".bad $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Unrecognized directive \".bad\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_UndefinedSymbolInstruction_Throws()
    {
        string[] lines = ["LDA #FOO"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Unable to resolve operand value \"FOO\" for instruction \"LDA\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_UndefinedSymbolConstant_Throws()
    {
        string[] lines = ["", "", "CONST = FOO"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(2, ex.Line);
        Assert.AreEqual("Could not evaluate expression \"FOO\" for constant \"CONST\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadByte_Throws()
    {
        string[] lines = ["", ".byte $00,$00,$%,$24"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(1, ex.Line);
        Assert.AreEqual("Could not evaluate expression \"$%\" in \".byte\" directive.", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeByte_Throws()
    {
        string[] lines = [".byte $FFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Value 65535 is out of range for \".byte\" directive. Expected 8-bit unsigned value (0 to 255).", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeByte_Throws()
    {
        string[] lines = [".byte -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Value -1 is out of range for \".byte\" directive. Expected 8-bit unsigned value (0 to 255).", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadWord_Throws()
    {
        string[] lines = ["", "", ".word $00,$00,$2352345626234652435,$24"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(2, ex.Line);
        Assert.AreEqual("Could not evaluate expression \"$2352345626234652435\" in \".word\" directive.", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeWord_Throws()
    {
        string[] lines = [".word $FFFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Value 1048575 is out of range for \".word\" directive. Expected 16-bit unsigned value (0 to 65535).", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeWord_Throws()
    {
        string[] lines = [".word -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Value -1 is out of range for \".word\" directive. Expected 16-bit unsigned value (0 to 65535).", ex.Error);
    }

    [TestMethod]
    public void Assemble_DuplicateSymbol_Throws()
    {
        string[] lines =
        [
            "FOO = $10",
            "",
            "FOO = $20"
        ];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(2, ex.Line);
        Assert.AreEqual("Duplicate symbol \"FOO\" is already defined.", ex.Error);
    }

    [TestMethod]
    public void Assemble_DanglingLabel_Throws()
    {
        string[] lines =
        [
            "",
            "DANGLING:"
        ];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(1, ex.Line);
        Assert.AreEqual("Label \"DANGLING\" is not followed by any addressable statement.", ex.Error);
    }

    [TestMethod]
    public void Assemble_RelativeOutOfRange_Throws()
    {
        List<string> lines = ["BNE End"];
        for (int i = 0; i < 128; i++)
            lines.Add("NOP");
        lines.Add("End:");
        lines.Add("NOP");

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble([.. lines]));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Relative branch offset 128 is out of range for instruction \"BNE\". Expected signed 8-bit value (-128 to 127).", ex.Error);
    }

    [TestMethod]
    public void Assemble_RelativeOutOfRangeMinus_Throws()
    {
        List<string> lines = ["Start:"];
        for (int i = 0; i < 128; i++)
            lines.Add("NOP");
        lines.Add("BNE Start");

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble([.. lines]));

        Assert.AreEqual(129, ex.Line);
        Assert.AreEqual("Relative branch offset -130 is out of range for instruction \"BNE\". Expected signed 8-bit value (-128 to 127).", ex.Error);
    }
}
