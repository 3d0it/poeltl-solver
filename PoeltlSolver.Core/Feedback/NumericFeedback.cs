namespace PoeltlSolver.Feedback;

public sealed record NumericFeedback(FeedbackColor Color, FeedbackDirection Direction)
{
    public override string ToString()
    {
        return Direction == FeedbackDirection.None
            ? Color.ToString()
            : $"{Color} {Direction}";
    }
}
