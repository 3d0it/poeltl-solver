using PoeltlSolver.Feedback;
using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public static class SoftCandidateEvaluator
{
    public static CandidateAnalysis Analyze(
        IReadOnlyList<Player> players,
        IReadOnlyList<FeedbackTurn> history)
    {
        var exactCandidates = CandidateFilter.Filter(players, history);
        var evaluations = players
            .Select(candidate => EvaluateCandidate(candidate, history))
            .OrderBy(evaluation => evaluation.Penalty)
            .ThenBy(evaluation => evaluation.Mismatches.Count)
            .ThenBy(evaluation => evaluation.Player.Name)
            .ToList();

        return new CandidateAnalysis(exactCandidates, evaluations);
    }

    private static CandidateEvaluation EvaluateCandidate(Player candidate, IReadOnlyList<FeedbackTurn> history)
    {
        var mismatches = new List<FeedbackMismatch>();

        foreach (var turn in history)
        {
            var expected = FeedbackEngine.Compare(turn.Guess, candidate);
            var entered = turn.Feedback;

            AddColorMismatch(
                mismatches,
                turn.Guess.Name,
                "Team",
                NormalizeTeam(expected.Team),
                NormalizeTeam(entered.Team),
                expected.Team.ToString(),
                entered.Team.ToString(),
                TeamPenalty(expected.Team, entered.Team));

            AddColorMismatch(mismatches, turn.Guess.Name, "Conference", expected.Conference, entered.Conference, 5.0);
            AddColorMismatch(mismatches, turn.Guess.Name, "Division", expected.Division, entered.Division, 5.0);
            AddColorMismatch(mismatches, turn.Guess.Name, "Position", expected.Position, entered.Position, 0.5);
            AddHeightMismatch(mismatches, turn.Guess, candidate, expected.Height, entered.Height);
            AddNumericMismatch(mismatches, turn.Guess.Name, "Age", expected.Age, entered.Age, 4.0);
            AddNumericMismatch(mismatches, turn.Guess.Name, "Number", expected.Number, entered.Number, 4.0);
        }

        return new CandidateEvaluation(
            candidate,
            mismatches.Sum(mismatch => mismatch.Penalty),
            mismatches);
    }

    private static void AddColorMismatch(
        List<FeedbackMismatch> mismatches,
        string guessName,
        string field,
        FeedbackColor expected,
        FeedbackColor entered,
        double penalty)
    {
        AddColorMismatch(
            mismatches,
            guessName,
            field,
            expected,
            entered,
            expected.ToString(),
            entered.ToString(),
            penalty);
    }

    private static void AddColorMismatch(
        List<FeedbackMismatch> mismatches,
        string guessName,
        string field,
        FeedbackColor comparableExpected,
        FeedbackColor comparableEntered,
        string expected,
        string entered,
        double penalty)
    {
        if (comparableExpected == comparableEntered)
        {
            return;
        }

        mismatches.Add(new FeedbackMismatch(guessName, field, expected, entered, penalty));
    }

    private static void AddHeightMismatch(
        List<FeedbackMismatch> mismatches,
        Player guess,
        Player candidate,
        NumericFeedback expected,
        NumericFeedback entered)
    {
        if (expected == entered || NormalizeHeight(expected) == NormalizeHeight(entered))
        {
            return;
        }

        var difference = Math.Abs(candidate.HeightInches - guess.HeightInches);
        var penalty = GetHeightPenalty(expected, entered, difference);

        mismatches.Add(new FeedbackMismatch(
            guess.Name,
            "Height",
            expected.ToString(),
            entered.ToString(),
            penalty));
    }

    private static void AddNumericMismatch(
        List<FeedbackMismatch> mismatches,
        string guessName,
        string field,
        NumericFeedback expected,
        NumericFeedback entered,
        double penalty)
    {
        if (expected == entered)
        {
            return;
        }

        mismatches.Add(new FeedbackMismatch(
            guessName,
            field,
            expected.ToString(),
            entered.ToString(),
            penalty));
    }

    private static double GetHeightPenalty(NumericFeedback expected, NumericFeedback entered, int difference)
    {
        if ((expected.Color == FeedbackColor.Green || entered.Color == FeedbackColor.Green) && difference <= 1)
        {
            return 0.75;
        }

        if (expected.Direction != FeedbackDirection.None && expected.Direction == entered.Direction)
        {
            return 1.0;
        }

        return 2.5;
    }

    private static FeedbackColor NormalizeTeam(FeedbackColor team)
    {
        return team == FeedbackColor.Yellow
            ? FeedbackColor.Gray
            : team;
    }

    private static NumericFeedback NormalizeHeight(NumericFeedback height)
    {
        if (height.Color == FeedbackColor.Green || height.Direction == FeedbackDirection.None)
        {
            return height;
        }

        return height with { Color = FeedbackColor.Gray };
    }

    private static double TeamPenalty(FeedbackColor expected, FeedbackColor entered)
    {
        return expected == FeedbackColor.Green || entered == FeedbackColor.Green
            ? 4.0
            : 1.5;
    }
}
