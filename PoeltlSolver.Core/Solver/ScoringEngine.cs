using PoeltlSolver.Feedback;
using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public static class ScoringEngine
{
    public static IReadOnlyList<GuessScore> ScoreGuesses(
        IReadOnlyList<Player> possibleGuesses,
        IReadOnlyList<Player> candidates)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var candidateSet = candidates.ToHashSet();

        return possibleGuesses
            .Select(guess => ScoreGuess(guess, candidates, candidateSet.Contains(guess)))
            .OrderByDescending(score => score.Entropy)
            .ThenBy(score => score.ExpectedRemaining)
            .ThenByDescending(score => score.IsCandidate)
            .ThenBy(score => score.Guess.Name)
            .ToList();
    }

    private static GuessScore ScoreGuess(Player guess, IReadOnlyList<Player> candidates, bool isCandidate)
    {
        var groups = candidates
            .GroupBy(target => PlayerFeedbackNormalizer.NormalizeForSolving(FeedbackEngine.Compare(guess, target)))
            .Select(group => group.Count())
            .ToList();

        var total = candidates.Count;
        var entropy = 0.0;
        var expectedRemaining = 0.0;

        foreach (var groupSize in groups)
        {
            var probability = (double)groupSize / total;
            entropy -= probability * Math.Log2(probability);
            expectedRemaining += probability * groupSize;
        }

        return new GuessScore(guess, entropy, expectedRemaining, isCandidate);
    }
}
