namespace Rombadil.Assembler;

public class AssemblerSyntaxException(int lineNumber, string line, string message) :
    Exception($"Line {lineNumber + 1}: {message}\n-> \"{line}\"");
