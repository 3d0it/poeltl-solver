namespace PoeltlSolver.Feedback;

public static class PlayerFeedbackNormalizer
{
    public static PlayerFeedback NormalizeForSolving(PlayerFeedback feedback)
    {
        return feedback with
        {
            Team = NormalizeTeam(feedback.Team),
            Position = FeedbackColor.Gray,
            Height = NormalizeHeight(feedback.Height),
        };
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
}
