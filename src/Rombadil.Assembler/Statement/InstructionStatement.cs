namespace Rombadil.Assembler;

public record struct InstructionStatement(CpuInstruction Instruction, CpuAdressingMode AdressingMode, string Expression);
