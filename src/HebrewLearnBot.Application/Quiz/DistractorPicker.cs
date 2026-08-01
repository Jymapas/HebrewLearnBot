using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HebrewLearnBot.Application.Quiz;

public sealed class DistractorPicker
{
    private readonly IRandomSource _randomSource;
    private readonly ILogger<DistractorPicker> _logger;

    public DistractorPicker(IRandomSource randomSource, ILogger<DistractorPicker> logger)
    {
        _randomSource = randomSource;
        _logger = logger;
    }

    public IReadOnlyList<QuizCandidate> Pick(
        QuizCandidate correct,
        IReadOnlyList<QuizCandidate> pool,
        QuizMode mode,
        int desiredCount)
    {
        if (desiredCount <= 0)
        {
            return Array.Empty<QuizCandidate>();
        }

        var (hardKeyOf, softKeyOf) = GetKeySelectors(mode);
        var correctHardKey = hardKeyOf(correct);

        // Слово-ответ и любые слова с тем же жёстким ключом (иначе кнопка задвоится) исключаем сразу.
        var eligiblePool = new List<QuizCandidate>(pool.Count);
        foreach (var candidate in pool)
        {
            if (candidate.WordId == correct.WordId)
            {
                continue;
            }

            if (string.Equals(hardKeyOf(candidate), correctHardKey, StringComparison.Ordinal))
            {
                continue;
            }

            eligiblePool.Add(candidate);
        }

        var selected = new List<QuizCandidate>();
        var selectedIds = new HashSet<int>();
        var usedHardKeys = new HashSet<string>(StringComparer.Ordinal) { correctHardKey };
        var usedSoftKeys = new HashSet<string>(StringComparer.Ordinal) { softKeyOf(correct) };

        // Тиры 1-5: строгий отбор, уважаем и жёсткий, и мягкий ключ.
        FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
            enforceSoftKey: true,
            tierPredicate: candidate => correct.Gender is not null
                && candidate.TopicId == correct.TopicId
                && candidate.PartOfSpeech == correct.PartOfSpeech
                && candidate.Gender == correct.Gender);

        FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
            enforceSoftKey: true,
            tierPredicate: candidate => candidate.TopicId == correct.TopicId
                && candidate.PartOfSpeech == correct.PartOfSpeech);

        FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
            enforceSoftKey: true,
            tierPredicate: candidate => candidate.TopicId == correct.TopicId);

        FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
            enforceSoftKey: true,
            tierPredicate: candidate => candidate.PartOfSpeech == correct.PartOfSpeech);

        FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
            enforceSoftKey: true,
            tierPredicate: static _ => true);

        // Релаксация: повторяем тиры «своя тема» / «своя часть речи» / «любое слово», жертвуя мягким ключом.
        if (selected.Count < desiredCount)
        {
            FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
                enforceSoftKey: false,
                tierPredicate: candidate => candidate.TopicId == correct.TopicId);

            FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
                enforceSoftKey: false,
                tierPredicate: candidate => candidate.PartOfSpeech == correct.PartOfSpeech);

            FillTier(selected, selectedIds, usedHardKeys, usedSoftKeys, eligiblePool, desiredCount, hardKeyOf, softKeyOf,
                enforceSoftKey: false,
                tierPredicate: static _ => true);
        }

        var optionCount = 1 + selected.Count;
        if (optionCount < QuizConstants.OptionCount)
        {
            _logger.LogWarning("Only {OptionCount} options built for word {WordId}", optionCount, correct.WordId);
        }

        return selected;
    }

    private void FillTier(
        List<QuizCandidate> selected,
        HashSet<int> selectedIds,
        HashSet<string> usedHardKeys,
        HashSet<string> usedSoftKeys,
        IReadOnlyList<QuizCandidate> pool,
        int desiredCount,
        Func<QuizCandidate, string> hardKeyOf,
        Func<QuizCandidate, string> softKeyOf,
        bool enforceSoftKey,
        Func<QuizCandidate, bool> tierPredicate)
    {
        if (selected.Count >= desiredCount)
        {
            return;
        }

        var tierCandidates = new List<QuizCandidate>();
        foreach (var candidate in pool)
        {
            if (selectedIds.Contains(candidate.WordId))
            {
                continue;
            }

            if (tierPredicate(candidate))
            {
                tierCandidates.Add(candidate);
            }
        }

        Shuffle(tierCandidates);

        foreach (var candidate in tierCandidates)
        {
            if (selected.Count >= desiredCount)
            {
                break;
            }

            var hardKey = hardKeyOf(candidate);
            if (usedHardKeys.Contains(hardKey))
            {
                continue;
            }

            var softKey = softKeyOf(candidate);
            if (enforceSoftKey && usedSoftKeys.Contains(softKey))
            {
                continue;
            }

            selected.Add(candidate);
            selectedIds.Add(candidate.WordId);
            usedHardKeys.Add(hardKey);
            usedSoftKeys.Add(softKey);
        }
    }

    private void Shuffle(IList<QuizCandidate> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _randomSource.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    private static (Func<QuizCandidate, string> Hard, Func<QuizCandidate, string> Soft) GetKeySelectors(QuizMode mode)
    {
        return mode switch
        {
            QuizMode.HeToRu or QuizMode.TranslitToRu =>
                (static c => c.TranslationNormalized, static c => c.HebrewNormalized),
            QuizMode.RuToHe or QuizMode.RuToHeWithTranslit =>
                (static c => c.HebrewNormalized, static c => c.TranslationNormalized),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported quiz mode.")
        };
    }
}
