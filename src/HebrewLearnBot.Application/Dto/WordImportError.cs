namespace HebrewLearnBot.Application.Dto;

public sealed record WordImportError(int LineNumber, string Message);
