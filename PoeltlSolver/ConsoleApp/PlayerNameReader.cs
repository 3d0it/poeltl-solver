using System.Text;
using PoeltlSolver.Models;

namespace PoeltlSolver.ConsoleApp;

public static class PlayerNameReader
{
    private const int SuggestionsToShow = 10;

    public static string? Read(string prompt, IReadOnlyList<Player> players)
    {
        Console.Write(prompt);

        if (Console.IsInputRedirected)
        {
            return Console.ReadLine();
        }

        var input = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return input.ToString();

                case ConsoleKey.Backspace:
                    RemoveLastCharacter(input);
                    break;

                case ConsoleKey.Tab:
                    CompleteOrShowSuggestions(prompt, input, players);
                    break;

                case ConsoleKey.Escape:
                    ReplaceInput(prompt, input, string.Empty);
                    break;

                default:
                    AppendCharacter(input, key.KeyChar);
                    break;
            }
        }
    }

    private static void CompleteOrShowSuggestions(
        string prompt,
        StringBuilder input,
        IReadOnlyList<Player> players)
    {
        var prefix = input.ToString();
        var matches = FindMatches(players, prefix);

        if (matches.Count == 0)
        {
            return;
        }

        if (matches.Count == 1)
        {
            ReplaceInput(prompt, input, matches[0]);
            return;
        }

        var commonPrefix = GetCommonPrefix(matches);
        if (commonPrefix.Length > prefix.Length)
        {
            ReplaceInput(prompt, input, commonPrefix);
            return;
        }

        PrintSuggestions(prompt, input.ToString(), matches);
    }

    private static List<string> FindMatches(IReadOnlyList<Player> players, string prefix)
    {
        return players
            .Select(player => player.Name)
            .Where(name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name)
            .ToList();
    }

    private static string GetCommonPrefix(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        var prefix = values[0];

        foreach (var value in values.Skip(1))
        {
            var length = 0;
            var maxLength = Math.Min(prefix.Length, value.Length);

            while (length < maxLength
                && char.ToUpperInvariant(prefix[length]) == char.ToUpperInvariant(value[length]))
            {
                length++;
            }

            prefix = prefix[..length];

            if (prefix.Length == 0)
            {
                break;
            }
        }

        return prefix;
    }

    private static void PrintSuggestions(string prompt, string input, IReadOnlyList<string> matches)
    {
        Console.WriteLine();

        foreach (var match in matches.Take(SuggestionsToShow))
        {
            Console.WriteLine($"- {match}");
        }

        if (matches.Count > SuggestionsToShow)
        {
            Console.WriteLine($"... and {matches.Count - SuggestionsToShow} more");
        }

        Console.Write(prompt);
        Console.Write(input);
    }

    private static void AppendCharacter(StringBuilder input, char character)
    {
        if (char.IsControl(character))
        {
            return;
        }

        input.Append(character);
        Console.Write(character);
    }

    private static void RemoveLastCharacter(StringBuilder input)
    {
        if (input.Length == 0)
        {
            return;
        }

        input.Length--;
        Console.Write("\b \b");
    }

    private static void ReplaceInput(string prompt, StringBuilder input, string value)
    {
        input.Clear();
        input.Append(value);

        Console.Write("\r");
        Console.Write(new string(' ', Console.BufferWidth - 1));
        Console.Write("\r");
        Console.Write(prompt);
        Console.Write(input);
    }
}
