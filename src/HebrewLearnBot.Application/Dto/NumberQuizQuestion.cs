using HebrewLearnBot.Domain.Numbers;

namespace HebrewLearnBot.Application.Dto;

public sealed record NumberQuizQuestion(HebrewNumberInfo Number, string QuestionText, IReadOnlyList<NumberQuizOption> Options, int CorrectIndex);
