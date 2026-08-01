using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Exceptions;
using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Domain.Entities;

public sealed class Word
{
    private const int MaxHebrewLength = 100;
    private const int MaxHebrewVocalizedLength = 200;
    private const int MaxTransliterationLength = 100;
    private const int MaxTranslationLength = 200;
    private const int MaxExternalKeyLength = 64;

    public int Id { get; private set; }
    public string Hebrew { get; private set; } = string.Empty;
    public string HebrewNormalized { get; private set; } = string.Empty;
    public string? HebrewVocalized { get; private set; }
    public string Transliteration { get; private set; } = string.Empty;
    public string TranslationRu { get; private set; } = string.Empty;
    public string TranslationNormalized { get; private set; } = string.Empty;
    public int TopicId { get; private set; }
    public PartOfSpeech PartOfSpeech { get; private set; }
    public Gender? Gender { get; private set; }
    public string? ExternalKey { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Word()
    {
    }

    public Word(
        string hebrew,
        string? hebrewVocalized,
        string transliteration,
        string translationRu,
        int topicId,
        PartOfSpeech partOfSpeech,
        Gender? gender,
        string? externalKey,
        DateTime createdAtUtc)
    {
        var strippedHebrew = ValidateHebrew(hebrew);
        var trimmedHebrewVocalized = ValidateHebrewVocalized(hebrewVocalized);
        var trimmedTransliteration = ValidateTransliteration(transliteration);
        var trimmedTranslationRu = ValidateTranslationRu(translationRu);
        ValidateTopicId(topicId);
        var trimmedExternalKey = ValidateExternalKey(externalKey);
        ValidateGender(gender, partOfSpeech);

        Hebrew = strippedHebrew;
        HebrewNormalized = TextNormalizer.NormalizeHebrew(hebrew);
        HebrewVocalized = trimmedHebrewVocalized;
        Transliteration = trimmedTransliteration;
        TranslationRu = trimmedTranslationRu;
        TranslationNormalized = TextNormalizer.NormalizeRussian(translationRu);
        TopicId = topicId;
        PartOfSpeech = partOfSpeech;
        Gender = gender;
        ExternalKey = trimmedExternalKey;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public void UpdateContent(string? hebrewVocalized, string transliteration, string translationRu, Gender? gender, DateTime updatedAtUtc)
    {
        var trimmedHebrewVocalized = ValidateHebrewVocalized(hebrewVocalized);
        var trimmedTransliteration = ValidateTransliteration(transliteration);
        var trimmedTranslationRu = ValidateTranslationRu(translationRu);
        ValidateGender(gender, PartOfSpeech);

        HebrewVocalized = trimmedHebrewVocalized;
        Transliteration = trimmedTransliteration;
        TranslationRu = trimmedTranslationRu;
        TranslationNormalized = TextNormalizer.NormalizeRussian(translationRu);
        Gender = gender;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void MoveToTopic(int topicId, DateTime updatedAtUtc)
    {
        ValidateTopicId(topicId);

        TopicId = topicId;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangePartOfSpeech(PartOfSpeech partOfSpeech, DateTime updatedAtUtc)
    {
        PartOfSpeech = partOfSpeech;

        // При смене части речи на не-существительное сбрасываем род, иначе состояние станет невалидным.
        if (partOfSpeech != PartOfSpeech.Noun && Gender is not null)
        {
            Gender = null;
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTime updatedAtUtc)
    {
        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string ValidateHebrew(string hebrew)
    {
        if (string.IsNullOrWhiteSpace(hebrew))
        {
            throw new DomainException("Hebrew spelling is required.");
        }

        var stripped = HebrewText.StripNiqqud(hebrew).Trim();

        if (!HebrewText.ContainsHebrewLetter(stripped))
        {
            throw new DomainException("Hebrew spelling must contain at least one Hebrew letter.");
        }

        if (stripped.Length > MaxHebrewLength)
        {
            throw new DomainException("Hebrew spelling must be 100 characters or shorter.");
        }

        return stripped;
    }

    private static string? ValidateHebrewVocalized(string? hebrewVocalized)
    {
        if (string.IsNullOrWhiteSpace(hebrewVocalized))
        {
            return null;
        }

        var trimmed = hebrewVocalized.Trim();

        if (trimmed.Length > MaxHebrewVocalizedLength)
        {
            throw new DomainException("Vocalized spelling must be 200 characters or shorter.");
        }

        return trimmed;
    }

    private static string ValidateTransliteration(string transliteration)
    {
        if (string.IsNullOrWhiteSpace(transliteration))
        {
            throw new DomainException("Transliteration is required.");
        }

        var trimmed = transliteration.Trim();

        if (!HebrewText.IsLatinTransliteration(trimmed))
        {
            throw new DomainException("Transliteration must contain only Latin letters, spaces, apostrophes and hyphens.");
        }

        if (trimmed.Length > MaxTransliterationLength)
        {
            throw new DomainException("Transliteration must be 100 characters or shorter.");
        }

        return trimmed;
    }

    private static string ValidateTranslationRu(string translationRu)
    {
        if (string.IsNullOrWhiteSpace(translationRu))
        {
            throw new DomainException("Russian translation is required.");
        }

        var trimmed = translationRu.Trim();

        if (trimmed.Length > MaxTranslationLength)
        {
            throw new DomainException("Russian translation must be 200 characters or shorter.");
        }

        return trimmed;
    }

    private static void ValidateTopicId(int topicId)
    {
        if (topicId <= 0)
        {
            throw new DomainException("TopicId must be positive.");
        }
    }

    private static string? ValidateExternalKey(string? externalKey)
    {
        if (string.IsNullOrWhiteSpace(externalKey))
        {
            return null;
        }

        var trimmed = externalKey.Trim();

        if (trimmed.Length > MaxExternalKeyLength)
        {
            throw new DomainException("External key must be 64 characters or shorter.");
        }

        return trimmed;
    }

    private static void ValidateGender(Gender? gender, PartOfSpeech partOfSpeech)
    {
        if (gender is not null && partOfSpeech != PartOfSpeech.Noun)
        {
            throw new DomainException("Gender is only allowed for nouns.");
        }
    }
}
