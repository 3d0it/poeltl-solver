using PoeltlSolver.Data;
using PoeltlSolver.Feedback;
using PoeltlSolver.Models;
using PoeltlSolver.Solver;

namespace PoeltlSolver.ConsoleApp;

public static class PoeltlConsole
{
    private const int MaxAttempts = 8;
    private const int SuggestionsToShow = 10;

    public static void Run(string csvPath)
    {
        var players = CsvPlayerLoader.Load(csvPath);
        var history = new List<FeedbackTurn>();

        Console.WriteLine($"Loaded {players.Count} players.");

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var analysis = SoftCandidateEvaluator.Analyze(players, history);
            var candidates = analysis.ActiveCandidates;

            Console.WriteLine();
            Console.WriteLine($"Attempt {attempt}/{MaxAttempts}");
            PrintCandidateCount(analysis);

            PrintSuggestions(players, candidates);

            var guess = ReadValidGuess(players);
            var feedback = ReadPlayerFeedback();

            Console.WriteLine();
            Console.WriteLine($"Guess:  {guess.Name}");
            PrintFeedback(feedback);

            if (feedback.IsAllGreen())
            {
                Console.WriteLine();
                Console.WriteLine($"Solved in {attempt} guesses.");
                return;
            }

            history.Add(new FeedbackTurn(guess, feedback));
            var updatedAnalysis = SoftCandidateEvaluator.Analyze(players, history);

            Console.WriteLine();
            PrintCandidateAnalysis(updatedAnalysis);
        }

        Console.WriteLine();
        Console.WriteLine("No more attempts left.");
    }

    private static Player ReadValidGuess(IReadOnlyList<Player> players)
    {
        while (true)
        {
            Console.WriteLine();
            var guessName = PlayerNameReader.Read("Guess player name: ", players);

            var guess = FindPlayer(players, guessName);
            if (guess is not null)
            {
                return guess;
            }

            Console.WriteLine($"Guess player not found: {guessName}");
        }
    }

    private static PlayerFeedback ReadPlayerFeedback()
    {
        Console.WriteLine("Enter feedback as 7 symbols: Team Conf Div Pos Ht Age #.");
        Console.WriteLine("Legend: = correct, - wrong, ~ close, > higher, < lower, + close higher, _ close lower.");
        Console.WriteLine("Example: -=-->_>");

        while (true)
        {
            Console.Write("Feedback: ");
            var input = Console.ReadLine()?.Trim();

            if (FeedbackInputParser.TryParse(input, out var feedback))
            {
                PrintDatasetLimitations(feedback);
                return feedback;
            }

            Console.WriteLine("Please enter exactly 7 valid symbols, for example: -=-->_>");
        }
    }

    private static void PrintDatasetLimitations(PlayerFeedback feedback)
    {
        if (feedback.Team == FeedbackColor.Yellow)
        {
            Console.WriteLine("Note: team yellow is approximated as \"not the guessed current team\" because previous-team history is not available in the current CSV.");
        }
    }

    private static void PrintCandidateCount(CandidateAnalysis analysis)
    {
        if (analysis.HasExactCandidates)
        {
            Console.WriteLine($"Current candidates: {analysis.ExactCandidates.Count}");
            return;
        }

        Console.WriteLine($"Current candidates: 0 exact; {analysis.BestNearMatches.Count} best near match(es)");
    }

    private static void PrintCandidateAnalysis(CandidateAnalysis analysis)
    {
        if (analysis.HasExactCandidates)
        {
            Console.WriteLine($"Remaining candidates: {analysis.ExactCandidates.Count}");
            PrintCandidates(analysis.ExactCandidates);
            return;
        }

        var nearMatches = analysis.BestNearMatches.Take(SuggestionsToShow).ToList();
        Console.WriteLine("No exact candidates; showing near matches.");

        if (nearMatches.Count == 0)
        {
            return;
        }

        Console.WriteLine($"Best near-match penalty: {nearMatches[0].Penalty:F2}");
        PrintCandidateEvaluations(nearMatches);
    }

    private static Player? FindPlayer(IReadOnlyList<Player> players, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return players.FirstOrDefault(player =>
            string.Equals(player.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void PrintSuggestions(IReadOnlyList<Player> allPlayers, IReadOnlyList<Player> candidates)
    {
        if (candidates.Count == 0)
        {
            Console.WriteLine("No candidates available for scoring.");
            return;
        }

        PrintScoreSection(
            "Top information guesses (can include non-candidates)",
            allPlayers,
            candidates);

        if (candidates.Count == allPlayers.Count)
        {
            return;
        }

        Console.WriteLine();
        PrintScoreSection(
            "Top candidate guesses (possible solutions only)",
            candidates,
            candidates);
    }

    private static void PrintScoreSection(
        string title,
        IReadOnlyList<Player> possibleGuesses,
        IReadOnlyList<Player> candidates)
    {
        var scores = ScoringEngine.ScoreGuesses(possibleGuesses, candidates).Take(SuggestionsToShow);

        Console.WriteLine(title);

        foreach (var score in scores)
        {
            Console.WriteLine(
                $"- {score.Guess.Name,-24} entropy {score.Entropy,5:F2} | expected remaining {score.ExpectedRemaining,6:F2}");
        }
    }

    private static void PrintFeedback(PlayerFeedback feedback)
    {
        Console.WriteLine();
        Console.WriteLine($"Team:       {feedback.Team}");
        Console.WriteLine($"Conference: {feedback.Conference}");
        Console.WriteLine($"Division:   {feedback.Division}");
        Console.WriteLine($"Position:   {feedback.Position}");
        Console.WriteLine($"Height:     {feedback.Height}");
        Console.WriteLine($"Age:        {feedback.Age}");
        Console.WriteLine($"Number:     {feedback.Number}");
    }

    private static void PrintCandidates(IReadOnlyList<Player> candidates)
    {
        foreach (var candidate in candidates)
        {
            Console.WriteLine($"- {candidate.Name} ({candidate.Team}, {candidate.Position}, {candidate.Height}, age {candidate.Age}, #{FormatNumber(candidate.Number)})");
        }
    }

    private static void PrintCandidateEvaluations(IReadOnlyList<CandidateEvaluation> evaluations)
    {
        foreach (var evaluation in evaluations)
        {
            var candidate = evaluation.Player;
            Console.WriteLine($"- {candidate.Name} ({candidate.Team}, {candidate.Position}, {candidate.Height}, age {candidate.Age}, #{FormatNumber(candidate.Number)}) | penalty {evaluation.Penalty:F2}");

            foreach (var mismatch in evaluation.Mismatches.Take(3))
            {
                Console.WriteLine($"  - {mismatch.Field} vs {mismatch.GuessName}: expected {mismatch.Expected}, entered {mismatch.Entered}");
            }
        }
    }

    private static string FormatNumber(JerseyNumber? number)
    {
        return number?.Text ?? "-";
    }
}
