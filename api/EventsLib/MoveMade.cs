namespace EventsLib;

public record MoveMade(
    Guid MatchId,
    int ActedSide,
    string Move,
    string NewFen,
    string NewPgn);