namespace Rombadil.Assembler;

public class CompilationAdressingModeResolver(CompilationResolver resolver)
{
    public (CpuAdressingMode, string) Resolve(CpuInstruction instruction, string operand)
    {
        var c = StringComparison.InvariantCultureIgnoreCase;

        if (string.IsNullOrEmpty(operand) || operand.Equals("A", c))
            return (CpuAdressingMode.Implied, string.Empty);

        if (operand.StartsWith('#'))
            return (CpuAdressingMode.Immediate, operand[1..]);

        if (operand.StartsWith('('))
        {
            if (operand.EndsWith(",X)", c))
                return (CpuAdressingMode.IndirectX, operand[1..^3]);

            if (operand.EndsWith("),Y", c))
                return (CpuAdressingMode.IndirectY, operand[1..^3]);

            if (operand.EndsWith(')'))
                return (CpuAdressingMode.Indirect, operand[1..^1]);
        }

        if (operand.EndsWith(",X", c))
            return ResolveZeroOrAbsolute(instruction, operand[..^2], CpuAdressingMode.ZeroPageX, CpuAdressingMode.AbsoluteX);

        if (operand.EndsWith(",Y", c))
            return ResolveZeroOrAbsolute(instruction, operand[..^2], CpuAdressingMode.ZeroPageY, CpuAdressingMode.AbsoluteY);

        if (CpuOpcodeMap.TryEncodeOpcode(instruction, CpuAdressingMode.Relative, out _))
            return (CpuAdressingMode.Relative, operand);

        return ResolveZeroOrAbsolute(instruction, operand, CpuAdressingMode.ZeroPage, CpuAdressingMode.Absolute);
    }

    private (CpuAdressingMode, string) ResolveZeroOrAbsolute(CpuInstruction instruction, string expression,
        CpuAdressingMode zpMode, CpuAdressingMode absMode)
    {
        if (resolver.TryResolveEquation(expression, out var val) && val <= 0xFF &&
            CpuOpcodeMap.TryEncodeOpcode(instruction, zpMode, out _))
            return (zpMode, expression);

        return (absMode, expression);
    }
}
