using PoeltlSolver.Models;

namespace PoeltlSolver.Solver;

public sealed record CandidateAnalysis(
    IReadOnlyList<Player> ExactCandidates,
    IReadOnlyList<CandidateEvaluation> Evaluations)
{
    private const double PenaltyTolerance = 0.0001;

    public bool HasExactCandidates => ExactCandidates.Count > 0;

    public IReadOnlyList<CandidateEvaluation> BestNearMatches
    {
        get
        {
            if (Evaluations.Count == 0)
            {
                return [];
            }

            var bestPenalty = Evaluations[0].Penalty;
            return Evaluations
                .Where(evaluation => Math.Abs(evaluation.Penalty - bestPenalty) <= PenaltyTolerance)
                .ToList();
        }
    }

    public IReadOnlyList<Player> ActiveCandidates => HasExactCandidates
        ? ExactCandidates
        : BestNearMatches.Select(evaluation => evaluation.Player).ToList();
}
