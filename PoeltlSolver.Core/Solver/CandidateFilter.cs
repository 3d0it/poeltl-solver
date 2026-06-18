using PoeltlSolver.Feedback;
using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public static class CandidateFilter
{
    public static IReadOnlyList<Player> Filter(
        IReadOnlyList<Player> players,
        Player guess,
        PlayerFeedback feedback)
    {
        return players
            .Where(candidate => MatchesFeedback(guess, candidate, feedback))
            .ToList();
    }

    public static IReadOnlyList<Player> Filter(
        IReadOnlyList<Player> players,
        IReadOnlyList<FeedbackTurn> history)
    {
        return history.Aggregate(
            players,
            (currentCandidates, turn) => Filter(currentCandidates, turn.Guess, turn.Feedback));
    }

    private static bool MatchesFeedback(Player guess, Player candidate, PlayerFeedback feedback)
    {
        var expected = PlayerFeedbackNormalizer.NormalizeForSolving(FeedbackEngine.Compare(guess, candidate));
        var entered = PlayerFeedbackNormalizer.NormalizeForSolving(feedback);

        return expected == entered;
    }
}
