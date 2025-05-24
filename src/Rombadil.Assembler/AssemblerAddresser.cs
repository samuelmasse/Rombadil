namespace Rombadil.Assembler;

internal class AssemblerAddresser(AssemblerResolver resolver)
{
    internal (CpuAddressingMode, string) Resolve(CpuInstruction instruction, string operand)
    {
        var c = StringComparison.InvariantCultureIgnoreCase;

        if (string.IsNullOrEmpty(operand) || operand.Equals("A", c))
            return (CpuAddressingMode.Implied, string.Empty);

        if (operand.StartsWith('#'))
            return (CpuAddressingMode.Immediate, operand[1..]);

        if (operand.StartsWith('('))
        {
            if (operand.EndsWith(",X)", c))
                return (CpuAddressingMode.IndirectX, operand[1..^3]);

            if (operand.EndsWith("),Y", c))
                return (CpuAddressingMode.IndirectY, operand[1..^3]);

            if (operand.EndsWith(')'))
                return (CpuAddressingMode.Indirect, operand[1..^1]);
        }

        if (operand.EndsWith(",X", c))
            return ResolveZeroOrAbsolute(instruction, operand[..^2], CpuAddressingMode.ZeroPageX, CpuAddressingMode.AbsoluteX);

        if (operand.EndsWith(",Y", c))
            return ResolveZeroOrAbsolute(instruction, operand[..^2], CpuAddressingMode.ZeroPageY, CpuAddressingMode.AbsoluteY);

        if (CpuOpcodeMap.TryEncodeOpcode(instruction, CpuAddressingMode.Relative, out _))
            return (CpuAddressingMode.Relative, operand);

        return ResolveZeroOrAbsolute(instruction, operand, CpuAddressingMode.ZeroPage, CpuAddressingMode.Absolute);
    }

    private (CpuAddressingMode, string) ResolveZeroOrAbsolute(CpuInstruction instruction, string expression,
        CpuAddressingMode zpMode, CpuAddressingMode absMode)
    {
        if (resolver.TryResolveEquation(expression, out var val) && val <= 0xFF &&
            CpuOpcodeMap.TryEncodeOpcode(instruction, zpMode, out _))
            return (zpMode, expression);

        return (absMode, expression);
    }
}
