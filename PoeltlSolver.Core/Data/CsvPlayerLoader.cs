using System.Globalization;
using PoeltlSolver.Models;

namespace PoeltlSolver.Data;

public static class CsvPlayerLoader
{
    public static IReadOnlyList<Player> Load(string path)
    {
        var fullPath = ResolvePath(path);
        var lines = File.ReadAllLines(fullPath);

        if (lines.Length <= 1)
        {
            return [];
        }

        var players = new List<Player>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = ParseCsvLine(line);

            players.Add(new Player(
                Name: columns[0],
                Team: columns[1],
                Conference: columns[2],
                Division: columns[3],
                Position: columns[4],
                Height: columns[5],
                HeightInches: int.Parse(columns[6], CultureInfo.InvariantCulture),
                Age: int.Parse(columns[7], CultureInfo.InvariantCulture),
                Number: JerseyNumber.Parse(columns[8])));
        }

        return players;
    }

    private static string ResolvePath(string path)
    {
        var currentDirectoryPath = Path.GetFullPath(path);
        if (File.Exists(currentDirectoryPath))
        {
            return currentDirectoryPath;
        }

        var parentDirectoryPath = Path.GetFullPath(Path.Combine("..", path));
        if (File.Exists(parentDirectoryPath))
        {
            return parentDirectoryPath;
        }

        throw new FileNotFoundException(
            $"Could not find CSV file at '{path}'. Generate it from the repository root with: python3 data/download_players.py --season 2025-26",
            path);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var columns = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Add('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                columns.Add(new string(current.ToArray()));
                current.Clear();
            }
            else
            {
                current.Add(character);
            }
        }

        columns.Add(new string(current.ToArray()));
        return columns;
    }
}
