using PoeltlSolver.Models;

namespace PoeltlSolver.Feedback;

public static class FeedbackEngine
{
    public static PlayerFeedback Compare(Player guess, Player target)
    {
        return new PlayerFeedback(
            Team: Exact(guess.Team, target.Team),
            Conference: Exact(guess.Conference, target.Conference),
            Division: Exact(guess.Division, target.Division),
            Position: ComparePosition(guess.Position, target.Position),
            Height: CompareNumeric(guess.HeightInches, target.HeightInches),
            Age: CompareNumeric(guess.Age, target.Age),
            Number: CompareJerseyNumber(guess.Number, target.Number));
    }

    private static FeedbackColor Exact(string guess, string target)
    {
        return string.Equals(guess, target, StringComparison.OrdinalIgnoreCase)
            ? FeedbackColor.Green
            : FeedbackColor.Gray;
    }

    private static NumericFeedback CompareNumeric(int guess, int target)
    {
        var direction = GetDirection(guess, target);

        if (guess == target)
        {
            return new NumericFeedback(FeedbackColor.Green, direction);
        }

        var color = Math.Abs(guess - target) <= 2
            ? FeedbackColor.Yellow
            : FeedbackColor.Gray;

        return new NumericFeedback(color, direction);
    }

    private static NumericFeedback CompareJerseyNumber(JerseyNumber? guess, JerseyNumber? target)
    {
        if (guess is null || target is null)
        {
            return new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None);
        }

        if (string.Equals(guess.Text, target.Text, StringComparison.OrdinalIgnoreCase))
        {
            return new NumericFeedback(FeedbackColor.Green, FeedbackDirection.None);
        }

        if (guess.NumericValue == target.NumericValue)
        {
            return new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None);
        }

        return CompareNumeric(guess.NumericValue, target.NumericValue);
    }

    private static FeedbackDirection GetDirection(int guess, int target)
    {
        if (target > guess)
        {
            return FeedbackDirection.Up;
        }

        if (target < guess)
        {
            return FeedbackDirection.Down;
        }

        return FeedbackDirection.None;
    }

    private static FeedbackColor ComparePosition(string guess, string target)
    {
        var guessPositions = SplitPositions(guess);
        var targetPositions = SplitPositions(target);

        if (guessPositions.SetEquals(targetPositions))
        {
            return FeedbackColor.Green;
        }

        return guessPositions.Overlaps(targetPositions)
            ? FeedbackColor.Yellow
            : FeedbackColor.Gray;
    }

    private static HashSet<string> SplitPositions(string position)
    {
        return position
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
