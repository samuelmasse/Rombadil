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
        Assert.AreEqual("Unable to resolve constant value \"$00+VAR2\" of \"VAR1\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameStartsWithNumber_Throws()
    {
        string[] lines = ["1VAR = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Names must begin with a letter \"1VAR\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameEmpty_Throws()
    {
        string[] lines = [" = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Names must not be empty.", ex.Error);
    }

    [TestMethod]
    public void Assemble_ConstantInvalidNameInvalidChar_Throws()
    {
        string[] lines = ["a_%$@im = $00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Names must be composed only of letters, numbers and underscores \"a_%$@im\".", ex.Error);
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
        Assert.AreEqual("Operand is outside of valid single byte range value \"257\" for LDA.", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeSingleByte_Throws()
    {
        string[] lines = ["lda -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand is outside of valid single byte range value \"-1\" for LDA.", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeTwoByte_Throws()
    {
        string[] lines = ["sta $FFFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand is outside of valid two byte range value \"1048575\" for STA.", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeTwoByte_Throws()
    {
        string[] lines = ["sta -$FFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Operand is outside of valid two byte range value \"-4095\" for STA.", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadAddressingMode_Throws()
    {
        string[] lines = ["lda ($00)"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("No opcode found for \"LDA\" with \"Indirect\" addressing.", ex.Error);
    }

    [TestMethod]
    public void Assemble_InvalidAddressingModeFormat_Throws()
    {
        string[] lines = ["lda ($00"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Unable to resolve operand value \"($00\" of \"LDA\".", ex.Error);
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
        Assert.AreEqual("Unable to resolve operand value \"FOO\" of \"LDA\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_UndefinedSymbolConstant_Throws()
    {
        string[] lines = ["", "", "CONST = FOO"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(2, ex.Line);
        Assert.AreEqual("Unable to resolve constant value \"FOO\" of \"CONST\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadByte_Throws()
    {
        string[] lines = ["", ".byte $00,$00,$%,$24"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(1, ex.Line);
        Assert.AreEqual("Unable to resolve .byte value \"$%\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeByte_Throws()
    {
        string[] lines = [".byte $FFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Out of range .byte value \"65535\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeByte_Throws()
    {
        string[] lines = [".byte -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Out of range .byte value \"-1\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_BadWord_Throws()
    {
        string[] lines = ["", "", ".word $00,$00,$2352345626234652435,$24"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(2, ex.Line);
        Assert.AreEqual("Unable to resolve .word value \"$2352345626234652435\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeWord_Throws()
    {
        string[] lines = [".word $FFFFF"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Out of range .word value \"1048575\".", ex.Error);
    }

    [TestMethod]
    public void Assemble_OutOfRangeNegativeWord_Throws()
    {
        string[] lines = [".word -1"];

        var ex = Assert.ThrowsExactly<Assembler6502Exception>(() => _ = new Assembler6502().Assemble(lines));

        Assert.AreEqual(0, ex.Line);
        Assert.AreEqual("Out of range .word value \"-1\".", ex.Error);
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
        Assert.AreEqual("Duplicate definition \"FOO\".", ex.Error);
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
        Assert.AreEqual("Dangling label \"DANGLING\".", ex.Error);
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
        Assert.AreEqual("Operand is outside of valid relative range value \"128\" for BNE.", ex.Error);
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
        Assert.AreEqual("Operand is outside of valid relative range value \"-130\" for BNE.", ex.Error);
    }
}
