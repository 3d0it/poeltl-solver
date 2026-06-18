using PoeltlSolver.Feedback;

namespace PoeltlSolver.Feedback;

public static class FeedbackInputParser
{
    public static bool TryParse(string? input, out PlayerFeedback feedback)
    {
        feedback = EmptyFeedback();

        if (string.IsNullOrWhiteSpace(input) || input.Length != 7)
        {
            return false;
        }

        if (!TryParseColor(input[0], out var team)
            || !TryParseColor(input[1], out var conference)
            || !TryParseColor(input[2], out var division)
            || !TryParseColor(input[3], out var position)
            || !TryParseNumeric(input[4], out var height)
            || !TryParseNumeric(input[5], out var age)
            || !TryParseNumeric(input[6], out var number))
        {
            return false;
        }

        feedback = new PlayerFeedback(
            Team: team,
            Conference: conference,
            Division: division,
            Position: position,
            Height: height,
            Age: age,
            Number: number);

        return true;
    }

    private static bool TryParseColor(char symbol, out FeedbackColor color)
    {
        switch (symbol)
        {
            case '-':
                color = FeedbackColor.Gray;
                return true;
            case '~':
                color = FeedbackColor.Yellow;
                return true;
            case '=':
                color = FeedbackColor.Green;
                return true;
            default:
                color = FeedbackColor.Gray;
                return false;
        }
    }

    private static bool TryParseNumeric(char symbol, out NumericFeedback feedback)
    {
        switch (symbol)
        {
            case '-':
                feedback = new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None);
                return true;
            case '~':
                feedback = new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.None);
                return true;
            case '=':
                feedback = new NumericFeedback(FeedbackColor.Green, FeedbackDirection.None);
                return true;
            case '>':
                feedback = new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up);
                return true;
            case '<':
                feedback = new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Down);
                return true;
            case '+':
                feedback = new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up);
                return true;
            case '_':
                feedback = new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Down);
                return true;
            default:
                feedback = new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None);
                return false;
        }
    }

    private static PlayerFeedback EmptyFeedback()
    {
        var emptyNumeric = new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None);

        return new PlayerFeedback(
            Team: FeedbackColor.Gray,
            Conference: FeedbackColor.Gray,
            Division: FeedbackColor.Gray,
            Position: FeedbackColor.Gray,
            Height: emptyNumeric,
            Age: emptyNumeric,
            Number: emptyNumeric);
    }
}
