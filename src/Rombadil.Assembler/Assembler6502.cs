namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] source) => new AssemblerCompilationUnit(source, new()).Compile();

    public byte[] AssembleOld(string[] source)
    {
        var output = new List<byte>();
        var opcodeMap = new OpcodeMap();

        for (int i = 0; i < source.Length; i++)
        {
            var line = source[i].Trim();
            int comment = line.IndexOf(';');
            if (comment >= 0)
                line = line[..comment].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.ToUpperInvariant().Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            if (!Enum.TryParse<Instruction>(parts[0].Trim(), out var instruction))
                throw new AssemblerSyntaxException(i, line, $"Unknown instruction '{parts[0]}'"); ;

            var mode = AdressingMode.Implied;
            int arg = 0;

            if (parts.Length > 1)
            {
                var operand = parts[1].Trim();

                if (operand.StartsWith('#'))
                {
                    mode = AdressingMode.Immediate;
                    arg = ReadNumber(operand[1..]);
                }
                else if (operand == "A")
                {
                    mode = AdressingMode.Implied;
                }
                else if (operand.Contains(','))
                {
                    if (operand.StartsWith('(') && operand.EndsWith(",X)"))
                    {
                        arg = ReadNumber(operand[1..^3]);
                        mode = AdressingMode.IndirectX;
                    }
                    else if (operand.StartsWith('(') && operand.EndsWith("),Y"))
                    {
                        arg = ReadNumber(operand[1..^3]);
                        mode = AdressingMode.IndirectY;
                    }
                    else if (operand.EndsWith(",X"))
                    {
                        arg = ReadNumber(operand[..^2]);
                        mode = arg > 0xFF ? AdressingMode.AbsoluteX : AdressingMode.ZeroPageX;
                    }
                    else if (operand.EndsWith(",Y"))
                    {
                        arg = ReadNumber(operand[..^2]);
                        mode = arg > 0xFF ? AdressingMode.AbsoluteY : AdressingMode.ZeroPageY;
                    }
                    else throw new AssemblerSyntaxException(i, line, $"Invalid operand format: '{operand}'");
                }
                else if (operand.StartsWith('(') && operand.EndsWith(')'))
                {
                    arg = ReadNumber(operand[1..^1]);
                    mode = AdressingMode.Indirect;
                }
                else
                {
                    arg = ReadNumber(operand);
                    mode = arg > 0xFF ? AdressingMode.Absolute : AdressingMode.ZeroPage;
                }
            }

            var opcode = opcodeMap.EncodeOpcode(instruction, mode);
            Console.WriteLine($"{instruction} {mode} {arg:X} {opcode}");

            int size = OperandSize(mode);

            if (size == 1 && (arg < 0 || arg > 0xFF))
                throw new AssemblerSyntaxException(i, line, $"Operand too large for 1-byte mode: {arg:X}");
            if (size == 2 && (arg < 0 || arg > 0xFFFF))
                throw new AssemblerSyntaxException(i, line, $"Operand out of 16-bit range: {arg:X}");

            output.Add((byte)opcode);

            if (size == 1)
                output.Add((byte)(arg & 0xFF));
            else if (size == 2)
            {
                output.Add((byte)(arg & 0xFF));
                output.Add((byte)((arg >> 8) & 0xFF));
            }
        }

        return output.ToArray();
    }

    private static int OperandSize(AdressingMode mode)
    {
        return mode switch
        {
            AdressingMode.Implied => 0,
            AdressingMode.Immediate => 1,
            AdressingMode.ZeroPage => 1,
            AdressingMode.ZeroPageX => 1,
            AdressingMode.ZeroPageY => 1,
            AdressingMode.Absolute => 2,
            AdressingMode.AbsoluteX => 2,
            AdressingMode.AbsoluteY => 2,
            AdressingMode.IndirectX => 1,
            AdressingMode.IndirectY => 1,
            _ => throw new NotImplementedException($"Unhandled mode: {mode}")
        };
    }

    private int ReadNumber(string str)
    {
        if (str.StartsWith('$'))
            return Convert.ToInt32(str[1..], 16);
        if (str.StartsWith('%'))
            return Convert.ToInt32(str[1..], 2);
        return Convert.ToInt32(str, 10);
    }
}

public record struct AssemblerLine(string Name, string Value, AssemblerLineType Type);
public enum AssemblerLineType
{
    Constant,
    Label,
    Operation
}
public record struct AssemblerInstruction(Instruction Instruction, AdressingMode AdressingMode, string Operand);
public record struct AssemblerTerm(string Value, AssemblerOperation Operation);
public enum AssemblerOperation
{
    Add,
    Subtract
}
public class AssemblerSyntaxException(int lineNumber, string line, string message) : Exception($"Line {lineNumber + 1}: {message}\n-> \"{line}\"");
