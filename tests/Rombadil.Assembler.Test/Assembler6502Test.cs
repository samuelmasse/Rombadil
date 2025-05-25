namespace Rombadil.Assembler.Test;

[TestClass]
public sealed class Assembler6502Test
{
    [TestMethod]
    public void Assemble_Empty_CorrectBytes()
    {
        string[] lines = [];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ImmediateLoadA_CorrectBytes()
    {
        string[] lines = ["LDA #$10"];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x10];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LDA_AddressingModes_CorrectOpcodes()
    {
        string[] lines =
        [
            "LDA #$44",        // Immediate
            "LDA $44",         // ZeroPage
            "LDA $44,X",       // ZeroPage,X
            "LDA $4400",       // Absolute
            "LDA $4400,X",     // Absolute,X
            "LDA $4400,Y",     // Absolute,Y
            "LDA ($44,X)",     // Indirect,X
            "LDA ($44),Y"      // Indirect,Y
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected =
        [
            0xA9, 0x44,             // Immediate
            0xA5, 0x44,             // ZeroPage
            0xB5, 0x44,             // ZeroPage,X
            0xAD, 0x00, 0x44,       // Absolute
            0xBD, 0x00, 0x44,       // Absolute,X
            0xB9, 0x00, 0x44,       // Absolute,Y
            0xA1, 0x44,             // (Indirect,X)
            0xB1, 0x44              // (Indirect),Y
        ];

        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_SpecialAddressingModes_CorrectOpcodes()
    {
        string[] lines =
        [
            "ASL A",            // Accumulator
            "NOP",              // Implied
            "BEQ Label",        // Relative
            "JMP ($1234)",      // Indirect
            "Label: RTS"        // Implied (target for BEQ)
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected =
        [
            0x0A,               // ASL A (Accumulator)
            0xEA,               // NOP (Implied)
            0xF0, 0x03,         // BEQ +3 to RTS (Relative)
            0x6C, 0x34, 0x12,   // JMP ($1234) (Indirect)
            0x60                // RTS (Implied)
        ];

        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_InstructionSpacing_AllFormatsAssembleCorrectly()
    {
        string[] lines =
        [
            " LdA\t(  $44  )  ,Y ", // Indirect,Y
            "LDA  ( $44,X  )",      // Indirect,X
            "jmP  ( $1234  ) ",     // Indirect
            "  \tLDX  #  $20 ",     // Immediate
            " STA  $4400   ,y",     // Absolute,Y
            "adc $4400 ,  x  ",     // Absolute,X
            "SBC $44 , X"           // ZeroPage,X
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected =
        [
            0xB1, 0x44,             // LDA ($44),Y
            0xA1, 0x44,             // LDA ($44,X)
            0x6C, 0x34, 0x12,       // JMP ($1234)
            0xA2, 0x20,             // LDX #$20
            0x99, 0x00, 0x44,       // STA $4400,Y
            0x7D, 0x00, 0x44,       // ADC $4400,X
            0xF5, 0x44              // SBC $44,X
        ];

        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ConstantDefinition_SingleConstant()
    {
        string[] lines =
        [
            "FOO = $2A",
            "LDA #FOO"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x2A];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ConstantAddition_AddsCorrectly()
    {
        string[] lines =
        [
            "FOO = $10",
            "BAR = $05",
            "LDA #FOO+BAR"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x15];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ConstantSubtraction_SubtractsCorrectly()
    {
        string[] lines =
        [
            "FOO = $10",
            "BAR = $05",
            "LDA #FOO-BAR"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x0B];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_Constant_MixedAddSubtract()
    {
        string[] lines =
        [
            "A = $20",
            "B = $08",
            "C = $04",
            "LDA #A+B-C"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x24]; // $20 + $08 - $04
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_Constant_LowHighByteSelectors()
    {
        string[] lines =
        [
            "ADDR = $1234",
            "LDA #<ADDR",
            "LDX #>ADDR"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x34, 0xA2, 0x12];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ByteDirective_EmitsCorrectBytes()
    {
        string[] lines =
        [
            ".byte $01, $02, $03"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x01, 0x02, 0x03];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_WordDirective_EmitsCorrectWords()
    {
        string[] lines =
        [
            ".word $1234, $ABCD"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x34, 0x12, 0xCD, 0xAB];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_MixedByteAndWordDirectives_EmitCorrectly()
    {
        string[] lines =
        [
            ".byte $10",
            ".word $1234",
            ".byte $20"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x10, 0x34, 0x12, 0x20];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_NumericNotation_AllFormatsProduceSameResult()
    {
        string[] lines =
        [
            "LDA #$FF",       // Hex
            "LDX #%10101010", // Binary
            "LDY #170"        // Decimal
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0xFF, 0xA2, 0xAA, 0xA0, 0xAA];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_MixedNumericFormats_ExpressionEvaluatesCorrectly()
    {
        string[] lines =
        [
            "LDA #$10 + %0001 + 5"  // 0x10 + 0x01 + 0x05 = 0x16
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x16];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelReference_ForwardJumpResolves()
    {
        string[] lines =
        [
            "JMP Target",
            "NOP",
            "Target: RTS"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x4C, 0x04, 0x00, 0xEA, 0x60]; // JMP to address 0x0004
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelReference_BackwardJumpResolves()
    {
        string[] lines =
        [
            "Loop: NOP",
            "JMP Loop"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xEA, 0x4C, 0x00, 0x00]; // JMP to address 0x0000
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelInWordDirective_ResolvesAddress()
    {
        string[] lines =
        [
            ".word Target",
            "NOP",
            "Target: RTS"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x03, 0x00, 0xEA, 0x60]; // Target is at 0x0003
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_BranchToLabel_RelativeOffsetIsCorrect()
    {
        string[] lines =
        [
            "BEQ Done",   // @0
            "NOP",        // @2
            "NOP",        // @3
            "Done: RTS"   // @4
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xF0, 0x02, 0xEA, 0xEA, 0x60]; // BEQ offset = 2 (to 0x04)
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelWithOffset_AddsCorrectly()
    {
        string[] lines =
        [
            "JMP Start+2",
            "NOP",
            "Start: NOP",
            "RTS"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x4C, 0x06, 0x00, 0xEA, 0xEA, 0x60]; // Jump to address 0x0006
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ZeroPageVsAbsolute_AddressingIsSelectedByValue()
    {
        string[] lines =
        [
            "LDA $10",      // ZeroPage
            "LDA $1234"     // Absolute
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA5, 0x10, 0xAD, 0x34, 0x12];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_DirectivesWithConstants_EmitCorrectBytes()
    {
        string[] lines =
        [
            "VAL = $42",
            ".byTe VAL",
            ".worD VAL+1"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x42, 0x43, 0x00];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelExpressionInDirective_ResolvesCorrectly()
    {
        string[] lines =
        [
            "Start: NOP",
            ".word Start+1"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xEA, 0x01, 0x00]; // Start is 0x0000, so word = 0x0001
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_CommentsAndEmptyLines_AreIgnored()
    {
        string[] lines =
        [
            "",                      // Empty line
            "; This is a comment",   // Full-line comment
            "LDA #$01   ; inline comment",
            "",                      // Another empty line
            "  ",                    // Whitespace only
            "STA $0200"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x01, 0x8D, 0x00, 0x02]; // LDA #$01, STA $0200
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ChainedConstants_EvaluateCorrectly()
    {
        string[] lines =
        [
            "A = $10",
            "B = A + 1",
            "C = B + 1",
            "LDA #C"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA9, 0x12]; // $10 + 1 + 1 = $12
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_LabelBeforeByteDirective_LabelAddressIsCorrect()
    {
        string[] lines =
        [
            "Data: .byte $01, $02",
            ".word Data"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x01, 0x02, 0x00, 0x00]; // Data is at 0x0000
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_BackwardBranch_RelativeOffsetIsNegative()
    {
        string[] lines =
        [
            "Loop: NOP",
            "BNE Loop"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xEA, 0xD0, 0xFD]; // BNE -3
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ByteDirective_WithSpacesStillCorrect()
    {
        string[] lines =
        [
            ".byte $01 ,  $02, $03 ,$04"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0x01, 0x02, 0x03, 0x04];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_WordDirectiveWithLabelOffset_ResolvesCorrectly()
    {
        string[] lines =
        [
            "Start: NOP",
            ".word Start + 1"
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xEA, 0x01, 0x00]; // Start = 0x0000, +1
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ZeroPageVsAbsolute_DetermineByResolvingVariables()
    {
        string[] lines =
        [
            "ZP1 = $08",
            "ZP2 = ZP1 + $10",
            "AB1 = $0F0F",
            "AB2 = AB1 + $01",
            "LDA ZP2",    // ZeroPage
            "LDA AB2"     // Absolute
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xA5, 0x18, 0xAD, 0x10, 0x0F];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_ZeroPageVsAbsolute_DetermineByNotResolvingVariables()
    {
        string[] lines =
        [
            "ZP1 = $08 + Label",
            "ZP2 = ZP1 + $10",
            "AB1 = $0F0F",
            "AB2 = AB1 + $01",
            "LDA ZP2",    // Has to be absolute because it relies on label
            "Label:",
            "LDA AB2"     // Absolute
        ];

        var binary = new Assembler6502().Assemble(lines);

        byte[] expected = [0xAD, 0x1B, 0x00, 0xAD, 0x10, 0x0F];
        CollectionAssert.AreEqual(expected, binary);
    }

    [TestMethod]
    public void Assemble_SegmentAndIncbin_CorrectBytes()
    {
        string[] lines =
        [
            ".segment \"HEADER\"",
            ".byte $01, $02, $03",
            ".segment \"CODE\"",
            "Start:",
            "LDA #$04",
            ".segment \"CHARS\"",
            ".incbin \"game.chr\"",
            ".segment \"VECTORS\"",
            ".word $05, Start, $06"
        ];

        var chrData = Enumerable.Range(0, 0x1000).Select(i => (byte)(i % 256)).ToArray();

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/game.chr", new MockFileData(chrData) }
        });

        var segments = new List<AssemblerSegment>
        {
            new("HEADER", 0x0000, 0x0010),
            new("CODE", 0x8000, 0x7FFA),
            new("VECTORS", 0xFFFA, 0x0006),
            new("CHARS", 0x0000, 0x2000)
        };

        var assembler = new Assembler6502(segments, fs);

        var binary = assembler.Assemble(lines);

        int length = segments.Sum(x => x.FileSize);
        Assert.AreEqual(length, binary.Length);

        var expected = new byte[length];

        // .byte $01, $02, $03
        expected[0x0000] = 0x01;
        expected[0x0001] = 0x02;
        expected[0x0002] = 0x03;

        // LDA #$04
        expected[0x0010] = 0xA9;
        expected[0x0011] = 0x04;

        // .incbin \"game.chr\"
        Array.Copy(chrData, 0, expected, 0x0010 + 0x7FFA + 0x0006, 0x1000);

        // .word $05, Start, $06
        expected[0x0010 + 0x7FFA + 0] = 0x05;
        expected[0x0010 + 0x7FFA + 1] = 0x00;
        expected[0x0010 + 0x7FFA + 2] = 0x00;
        expected[0x0010 + 0x7FFA + 3] = 0x80;
        expected[0x0010 + 0x7FFA + 4] = 0x06;
        expected[0x0010 + 0x7FFA + 5] = 0x00;

        CollectionAssert.AreEqual(expected, binary);
    }
}
