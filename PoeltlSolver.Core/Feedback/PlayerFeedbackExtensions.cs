namespace PoeltlSolver.Feedback;

public static class PlayerFeedbackExtensions
{
    public static bool IsAllGreen(this PlayerFeedback feedback)
    {
        return feedback.Team == FeedbackColor.Green
            && feedback.Conference == FeedbackColor.Green
            && feedback.Division == FeedbackColor.Green
            && feedback.Position == FeedbackColor.Green
            && feedback.Height.Color == FeedbackColor.Green
            && feedback.Age.Color == FeedbackColor.Green
            && feedback.Number.Color == FeedbackColor.Green;
    }
}
