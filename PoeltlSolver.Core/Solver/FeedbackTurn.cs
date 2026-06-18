using PoeltlSolver.Feedback;
using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public sealed record FeedbackTurn(Player Guess, PlayerFeedback Feedback);
