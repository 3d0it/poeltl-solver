namespace PoeltlSolver.Models;

public sealed record Player(
    string Name,
    string Team,
    string Conference,
    string Division,
    string Position,
    string Height,
    int HeightInches,
    int Age,
    JerseyNumber? Number);
