namespace Rombadil.Script;

public static partial class Script
{
    public static void Echo(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void Info(string message)
    {
        Echo($"[INFO] {message}", ConsoleColor.Cyan);
    }

    public static void Success(string message)
    {
        Echo($"[SUCCESS] {message}", ConsoleColor.Green);
    }

    public static void Fail(string message)
    {
        Echo($"[FAIL] {message}", ConsoleColor.Red);
    }
}
