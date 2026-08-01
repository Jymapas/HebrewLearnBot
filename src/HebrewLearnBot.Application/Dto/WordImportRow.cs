using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Dto;

public sealed record WordImportRow(
    int LineNumber,
    string Hebrew,
    string? HebrewVocalized,
    string Transliteration,
    string TranslationRu,
    string TopicName,
    Gender? Gender,
    PartOfSpeech PartOfSpeech,
    string? ExternalKey);
