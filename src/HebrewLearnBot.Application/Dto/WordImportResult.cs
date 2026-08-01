namespace HebrewLearnBot.Application.Dto;

public sealed record WordImportResult(
    int Added,
    int Updated,
    int Skipped,
    IReadOnlyList<WordImportError> Errors);
