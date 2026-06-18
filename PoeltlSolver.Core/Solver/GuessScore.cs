using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public sealed record GuessScore(
    Player Guess,
    double Entropy,
    double ExpectedRemaining,
    bool IsCandidate);
