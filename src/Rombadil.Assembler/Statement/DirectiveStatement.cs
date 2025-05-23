namespace Rombadil.Assembler;

public record struct DirectiveStatement(DirectiveType Type, string[] Expressions);
