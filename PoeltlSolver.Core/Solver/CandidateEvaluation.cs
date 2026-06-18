using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public sealed record CandidateEvaluation(
    Player Player,
    double Penalty,
    IReadOnlyList<FeedbackMismatch> Mismatches);
