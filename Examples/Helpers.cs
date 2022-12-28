using System.Runtime.CompilerServices;

namespace Examples;

internal static class Helpers
{
    // General
    internal record Wizard(string Name, int NbSpells);
    internal static void Log(object value) => Console.WriteLine(value);
    internal static void RunExample(string name, Action action)
    {
        Log($"\n[ {name} ]");
        action();
    }
    internal static void Assert(bool expected, [CallerArgumentExpression("expected")] string name = "")
    {
        if (expected) return;
        Err($"Assertion '{name}' failed.").ThrowIfErr();
    }


    // Scenario
    internal static void ExternalDbCall(bool simulateException)
    {
        if (simulateException)
        {
            throw new Exception("database connection error");
        }
    }
    internal static string? GetFilepathFromUserMaybeNull(double flip)
    {
        // assume there is a user interaction, where the user might:
        // * cancel and not provide any path,
        // * provide a nonnumeric-file path that does not have numbers,
        // * provide a negative-file path that has negative values which will be a problem later, or
        // * provide a good filepath, that works.
        return flip switch
        {
            < 0.25 => null,
            < 0.50 => "nonnumeric-file",
            < 0.75 => "negative-file",
            _ => "good-file",
        };
    }
    internal static Opt<string> GetFilepathFromUser(double flip)
    {
        return flip switch
        {
            < 0.25 => None<string>(),
            < 0.50 => Some<string>("nonnumeric-file"),
            < 0.75 => Some<string>("negative-file"),
            _ => Some<string>("good-file"),
        };
    }
    internal static int[] RiskyParse(string filepath)
    {
        return filepath switch
        {
            "good-file" => Enumerable.Range(0, 10).ToArray(),
            "negative-file" => Enumerable.Range(-5, 5).ToArray(),
            "nonnumeric-file" => throw new ArgumentException($"error while parsing {filepath}"),
            _ => throw new ArgumentException("unknown file"),
        };
    }
    internal static int LogAndGetSumAmounts(int[] numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] < 0)
                throw new ArgumentException($"Numbers must be nonnegative, but found {numbers[i]}");
            sum += numbers[i];
        }
        Log($"Total amount: {sum}");
        return sum;
    }
}
