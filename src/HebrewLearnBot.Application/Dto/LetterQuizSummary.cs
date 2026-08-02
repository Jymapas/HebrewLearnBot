namespace HebrewLearnBot.Application.Dto;

public sealed record LetterQuizSummary(int AskedCount, int CorrectCount, IReadOnlyList<LetterBrief> WrongLetters);
