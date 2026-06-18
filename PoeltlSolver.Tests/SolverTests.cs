using PoeltlSolver.Data;
using PoeltlSolver.Feedback;
using PoeltlSolver.Models;
using PoeltlSolver.Solver;
using Xunit;

namespace PoeltlSolver.Tests;

public sealed class SolverTests
{
    [Fact]
    public void PositionExactMatchIgnoresOrder()
    {
        var guess = Player("Guess", position: "G-F");
        var target = Player("Target", position: "F-G");

        var feedback = FeedbackEngine.Compare(guess, target);

        Assert.Equal(FeedbackColor.Green, feedback.Position);
    }

    [Fact]
    public void PositionOverlapIsYellowOnlyWhenRolesOverlap()
    {
        Assert.Equal(
            FeedbackColor.Yellow,
            FeedbackEngine.Compare(Player("Guess", position: "G"), Player("Target", position: "G-F")).Position);

        Assert.Equal(
            FeedbackColor.Gray,
            FeedbackEngine.Compare(Player("Guess", position: "G"), Player("Target", position: "F")).Position);
    }

    [Fact]
    public void NumericCloseUsesDistanceAtMostTwo()
    {
        var close = FeedbackEngine.Compare(
            Player("Guess", heightInches: 74, age: 25, number: "3"),
            Player("Target", heightInches: 76, age: 27, number: "5"));

        Assert.Equal(new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up), close.Height);
        Assert.Equal(new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up), close.Age);
        Assert.Equal(new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up), close.Number);

        var far = FeedbackEngine.Compare(
            Player("Guess", heightInches: 74, age: 25, number: "3"),
            Player("Target", heightInches: 77, age: 28, number: "6"));

        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up), far.Height);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up), far.Age);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up), far.Number);
    }

    [Fact]
    public void CompactFeedbackInputParsesFieldOrderAndNumericSymbols()
    {
        var parsed = FeedbackInputParser.TryParse("-=--<+<", out var feedback);

        Assert.True(parsed);
        Assert.Equal(FeedbackColor.Gray, feedback.Team);
        Assert.Equal(FeedbackColor.Green, feedback.Conference);
        Assert.Equal(FeedbackColor.Gray, feedback.Division);
        Assert.Equal(FeedbackColor.Gray, feedback.Position);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Down), feedback.Height);
        Assert.Equal(new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up), feedback.Age);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Down), feedback.Number);
    }

    [Fact]
    public void CsvLoaderReadsCurrentDataset()
    {
        var players = LoadPlayers();

        Assert.NotEmpty(players);
        Assert.Contains(players, player => player.Name == "Quentin Grimes");
    }

    [Fact]
    public void CsvLoaderMissingFileExplainsHowToGenerateDataset()
    {
        var exception = Assert.Throws<FileNotFoundException>(
            () => CsvPlayerLoader.Load("data/missing-poeltl-players.csv"));

        Assert.Contains("python3 data/download_players.py --season 2025-26", exception.Message);
    }

    [Fact]
    public void CsvLoaderPreservesDoubleZeroJerseyNumbers()
    {
        var csvPath = WriteTempCsv(
            "Name,Team,Conference,Division,Position,Height,HeightInches,Age,Number",
            "Double Zero,AAA,East,Atlantic,G,6-2,74,25,00",
            "Legacy Seven,AAA,East,Atlantic,G,6-2,74,25,7.0",
            "Legacy Zero,AAA,East,Atlantic,G,6-2,74,25,0.0",
            "Missing,AAA,East,Atlantic,G,6-2,74,25,");

        var players = CsvPlayerLoader.Load(csvPath);

        Assert.Equal("00", RequiredPlayer(players, "Double Zero").Number?.Text);
        Assert.Equal(0, RequiredPlayer(players, "Double Zero").Number?.NumericValue);
        Assert.Equal("7", RequiredPlayer(players, "Legacy Seven").Number?.Text);
        Assert.Equal("0", RequiredPlayer(players, "Legacy Zero").Number?.Text);
        Assert.Null(RequiredPlayer(players, "Missing").Number);
    }

    [Fact]
    public void JerseyNumberFeedbackUsesTextForExactMatchAndNumericValueForDirection()
    {
        var exact = FeedbackEngine.Compare(
            Player("Guess", number: "00"),
            Player("Target", number: "00"));
        var sameNumericValue = FeedbackEngine.Compare(
            Player("Guess", number: "0"),
            Player("Target", number: "00"));
        var closeHigher = FeedbackEngine.Compare(
            Player("Guess", number: "00"),
            Player("Target", number: "2"));
        var missing = FeedbackEngine.Compare(
            Player("Guess", number: null),
            Player("Target", number: "00"));

        Assert.Equal(new NumericFeedback(FeedbackColor.Green, FeedbackDirection.None), exact.Number);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None), sameNumericValue.Number);
        Assert.Equal(new NumericFeedback(FeedbackColor.Yellow, FeedbackDirection.Up), closeHigher.Number);
        Assert.Equal(new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.None), missing.Number);
    }

    [Fact]
    public void CandidateFilterToleratesHeightColorDriftWhenDirectionMatches()
    {
        var players = LoadPlayers();
        var riley = RequiredPlayer(players, "Riley Minix");

        Assert.True(FeedbackInputParser.TryParse("-=--<+<", out var feedback));

        var candidates = CandidateFilter.Filter(players, riley, feedback);

        Assert.Contains(candidates, player => player.Name == "Quentin Grimes");
    }

    [Fact]
    public void KnownGrimesScenarioKeepsQuentinGrimes()
    {
        var players = LoadPlayers();
        IReadOnlyList<Player> candidates = players;

        candidates = ApplyStrictFeedback(candidates, players, "Riley Minix", "-=--<+<");
        Assert.Contains(candidates, player => player.Name == "Quentin Grimes");

        candidates = ApplyStrictFeedback(candidates, players, "Trae Young", "-=-=+_+");
        Assert.Contains(candidates, player => player.Name == "Quentin Grimes");

        candidates = ApplyStrictFeedback(candidates, players, "Immanuel Quickley", "-===+_=");
        Assert.Contains(candidates, player => player.Name == "Quentin Grimes");
    }

    [Fact]
    public void KnownJaylenBrownScenarioKeepsJaylenBrown()
    {
        var players = LoadPlayers();
        IReadOnlyList<Player> candidates = players;

        candidates = ApplyStrictFeedback(candidates, players, "Precious Achiuwa", "----_>_");
        Assert.Contains(candidates, player => player.Name == "Jaylen Brown");

        candidates = ApplyStrictFeedback(candidates, players, "Buddy Hield", "-=-=+<_");
        Assert.Contains(candidates, player => player.Name == "Jaylen Brown");
        Assert.DoesNotContain(candidates, player => player.Name == "Caris LeVert");
        Assert.DoesNotContain(candidates, player => player.Name == "Dennis Schröder");
        Assert.DoesNotContain(candidates, player => player.Name == "Kyle Lowry");

        candidates = ApplyStrictFeedback(candidates, players, "Caris LeVert", "-=-~___");
        var candidate = Assert.Single(candidates);
        Assert.Equal("Jaylen Brown", candidate.Name);
    }

    [Fact]
    public void SoftAnalysisUsesExactCandidatesWhenAvailable()
    {
        var players = LoadPlayers();
        var history = new[]
        {
            Turn(players, "Riley Minix", "-=--<+<"),
        };

        var analysis = SoftCandidateEvaluator.Analyze(players, history);

        Assert.True(analysis.HasExactCandidates);
        Assert.Contains(analysis.ExactCandidates, player => player.Name == "Quentin Grimes");
    }

    [Fact]
    public void SoftAnalysisFallsBackToNearMatchesWhenExactCandidatesCollapse()
    {
        var players = LoadPlayers();
        var history = new[]
        {
            Turn(players, "Riley Minix", "-=--<+<"),
            Turn(players, "Coby White", "-=-===+"),
        };

        var analysis = SoftCandidateEvaluator.Analyze(players, history);

        Assert.False(analysis.HasExactCandidates);
        var best = analysis.Evaluations[0];
        Assert.Equal("Quentin Grimes", best.Player.Name);
        Assert.Contains(analysis.BestNearMatches, evaluation => evaluation.Player.Name == "Quentin Grimes");
        Assert.Contains(
            best.Mismatches,
            mismatch => mismatch.GuessName == "Coby White"
                && mismatch.Field == "Height"
                && mismatch.Expected == "Yellow Up"
                && mismatch.Entered == "Green");
    }

    [Fact]
    public void TeamGrayKeepsDifferentCurrentTeams()
    {
        var guess = Player("Guess", team: "ATL");
        var candidate = Player("Candidate", team: "BOS");
        var feedback = FeedbackEngine.Compare(guess, candidate);

        var candidates = CandidateFilter.Filter([candidate], guess, feedback);

        Assert.Single(candidates);
    }

    [Fact]
    public void TeamYellowKeepsDifferentCurrentTeams()
    {
        var guess = Player("Guess", team: "ATL");
        var candidate = Player("Candidate", team: "BOS");
        var feedback = FeedbackEngine.Compare(guess, candidate) with { Team = FeedbackColor.Yellow };

        var candidates = CandidateFilter.Filter([candidate], guess, feedback);

        Assert.Single(candidates);
    }

    [Fact]
    public void TeamYellowExcludesSameCurrentTeam()
    {
        var guess = Player("Guess", team: "ATL");
        var candidate = Player("Candidate", team: "ATL");
        var feedback = FeedbackEngine.Compare(guess, candidate) with { Team = FeedbackColor.Yellow };

        var candidates = CandidateFilter.Filter([candidate], guess, feedback);

        Assert.Empty(candidates);
    }

    [Fact]
    public void TeamGreenRemainsStrict()
    {
        var guess = Player("Guess", team: "ATL");
        var matchingCandidate = Player("Matching", team: "ATL");
        var differentCandidate = Player("Different", team: "BOS");
        var feedback = FeedbackEngine.Compare(guess, matchingCandidate);

        var candidates = CandidateFilter.Filter([matchingCandidate, differentCandidate], guess, feedback);

        var candidate = Assert.Single(candidates);
        Assert.Equal("Matching", candidate.Name);
    }

    [Fact]
    public void TeamYellowStillCombinesWithOtherFeedbackFields()
    {
        var guess = Player("Guess", team: "ATL", conference: "East", division: "Southeast");
        var goodCandidate = Player("Good", team: "BOS", conference: "East", division: "Atlantic");
        var badCandidate = Player("Bad", team: "DEN", conference: "West", division: "Northwest");

        var feedback = FeedbackEngine.Compare(guess, goodCandidate) with { Team = FeedbackColor.Yellow };
        var candidates = CandidateFilter.Filter([goodCandidate, badCandidate], guess, feedback);

        var candidate = Assert.Single(candidates);
        Assert.Equal("Good", candidate.Name);
    }

    [Fact]
    public void ScoringUsesNormalizedFeedbackGroups()
    {
        var guess = Player("Guess", team: "ATL", heightInches: 74);
        var nearHeightCandidate = Player("Near", team: "BOS", heightInches: 76);
        var farHeightCandidate = Player("Far", team: "BOS", heightInches: 77);

        var score = ScoringEngine.ScoreGuesses([guess], [nearHeightCandidate, farHeightCandidate])[0];

        Assert.Equal(0.0, score.Entropy, precision: 6);
        Assert.Equal(2.0, score.ExpectedRemaining, precision: 6);
    }

    [Fact]
    public void CandidateFilterDoesNotFallbackWhenFeedbackMatchesNoCandidates()
    {
        var candidates = new[]
        {
            Player("A"),
            Player("B"),
            Player("C"),
        };

        var impossibleFeedback = new PlayerFeedback(
            Team: FeedbackColor.Green,
            Conference: FeedbackColor.Gray,
            Division: FeedbackColor.Gray,
            Position: FeedbackColor.Gray,
            Height: new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up),
            Age: new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up),
            Number: new NumericFeedback(FeedbackColor.Gray, FeedbackDirection.Up));

        var filteredCandidates = CandidateFilter.Filter(candidates, candidates[0], impossibleFeedback);

        Assert.Empty(filteredCandidates);
    }

    private static IReadOnlyList<Player> LoadPlayers()
    {
        return CsvPlayerLoader.Load(FindDatasetPath());
    }

    private static IReadOnlyList<Player> ApplyStrictFeedback(
        IReadOnlyList<Player> candidates,
        IReadOnlyList<Player> players,
        string guessName,
        string feedbackInput)
    {
        var guess = RequiredPlayer(players, guessName);

        Assert.True(FeedbackInputParser.TryParse(feedbackInput, out var feedback));

        return CandidateFilter.Filter(candidates, guess, feedback);
    }

    private static FeedbackTurn Turn(
        IReadOnlyList<Player> players,
        string guessName,
        string feedbackInput)
    {
        var guess = RequiredPlayer(players, guessName);

        Assert.True(FeedbackInputParser.TryParse(feedbackInput, out var feedback));

        return new FeedbackTurn(guess, feedback);
    }

    private static string FindDatasetPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var datasetPath = Path.Combine(directory.FullName, "data", "poeltl_players.csv");
            if (File.Exists(datasetPath))
            {
                return datasetPath;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find data/poeltl_players.csv from the test output directory.");
    }

    private static Player RequiredPlayer(IReadOnlyList<Player> players, string name)
    {
        return players.FirstOrDefault(player => player.Name == name)
            ?? throw new InvalidOperationException($"Missing player: {name}");
    }

    private static Player Player(
        string name,
        string team = "AAA",
        string conference = "East",
        string division = "Atlantic",
        string position = "G",
        string height = "6-2",
        int heightInches = 74,
        int age = 25,
        string? number = "1")
    {
        return new Player(
            Name: name,
            Team: team,
            Conference: conference,
            Division: division,
            Position: position,
            Height: height,
            HeightInches: heightInches,
            Age: age,
            Number: JerseyNumber.Parse(number));
    }

    private static string WriteTempCsv(params string[] lines)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        File.WriteAllLines(path, lines);
        return path;
    }
}
