using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Dto;

public sealed record WordBrief(
    int WordId,
    string Hebrew,
    string? HebrewVocalized,
    string Transliteration,
    string TranslationRu,
    Gender? Gender,
    string TopicName);
