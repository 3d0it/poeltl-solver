namespace PoeltlSolver.Solver;

public sealed record FeedbackMismatch(
    string GuessName,
    string Field,
    string Expected,
    string Entered,
    double Penalty);
