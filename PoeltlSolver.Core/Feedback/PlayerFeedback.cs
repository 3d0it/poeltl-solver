namespace PoeltlSolver.Feedback;

public sealed record PlayerFeedback(
    FeedbackColor Team,
    FeedbackColor Conference,
    FeedbackColor Division,
    FeedbackColor Position,
    NumericFeedback Height,
    NumericFeedback Age,
    NumericFeedback Number);
