using System.Globalization;
using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.Numbers;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Numbers;

namespace HebrewLearnBot.Application.UseCases.Numbers;

public sealed class BuildNextNumberQuestion
{
    private readonly INumberProgressRepository _repository;
    private readonly WeightedPicker _picker;
    private readonly IClock _clock;

    public BuildNextNumberQuestion(INumberProgressRepository repository, IRandomSource randomSource, IClock clock)
    {
        _repository = repository;
        _picker = new WeightedPicker(randomSource);
        _clock = clock;
    }

    public async Task<NumberQuizQuestion> ExecuteAsync(
        long userId,
        NumberQuizMode mode,
        IReadOnlyList<int> recentNumbers,
        int askedCount,
        int correctCount,
        CancellationToken cancellationToken)
    {
        var maxValue = NumberQuizRules.MaxValue(mode);
        var progress = (await _repository.LoadAsync(userId, cancellationToken))
            .Where(item => item.UserId == userId && item.Mode == mode)
            .ToDictionary(item => item.Value);
        var catalog = HebrewNumbers.All.Take(maxValue + 1).ToList();
        var recent = recentNumbers.TakeLast(5).ToHashSet();
        var candidates = catalog.Where(item => !recent.Contains(item.Value)).ToList();
        if (candidates.Count == 0)
        {
            candidates = catalog;
        }

        var nowUtc = _clock.UtcNow;
        var weights = candidates.Select(item => GetWeight(progress.GetValueOrDefault(item.Value), nowUtc)).ToList();
        var correct = candidates[_picker.PickIndex(weights)];
        var options = new List<NumberQuizOption>();
        var correctIndex = -1;
        var optionCount = NumberQuizRules.OptionCount(mode);
        if (optionCount > 0)
        {
            var distractors = catalog.Where(item => item.Value != correct.Value).ToList();
            _picker.Shuffle(distractors);
            var optionNumbers = distractors.Take(optionCount - 1).Append(correct).ToList();
            _picker.Shuffle(optionNumbers);
            correctIndex = optionNumbers.FindIndex(item => item.Value == correct.Value);
            options = optionNumbers.Select(item => new NumberQuizOption(item.Value, BuildLabel(item, mode))).ToList();
        }

        return new NumberQuizQuestion(correct, BuildQuestionText(correct, mode, askedCount, correctCount), options, correctIndex);
    }

    private static double GetWeight(NumberProgress? progress, DateTime nowUtc)
    {
        if (progress is null)
        {
            return 5.0;
        }

        var weight = progress.IsLearned ? 0.5 : 2.0;
        if (progress.LastAnswerCorrect == false)
        {
            weight += 6.0;
        }

        if (progress.LastShownAtUtc is { } lastShown)
        {
            weight *= 1.0 + Math.Clamp((nowUtc - lastShown).TotalDays / 7.0, 0.0, 2.0);
        }

        return weight;
    }

    private static string BuildLabel(HebrewNumberInfo number, NumberQuizMode mode)
    {
        return mode switch
        {
            NumberQuizMode.HebrewToNumber or NumberQuizMode.TransliterationToNumber => number.Value.ToString(CultureInfo.InvariantCulture),
            NumberQuizMode.NumberToHebrewWithTransliteration => $"{BidiText.Isolate(number.Hebrew)} ({number.Transliteration})",
            _ => number.Hebrew
        };
    }

    private static string BuildQuestionText(HebrewNumberInfo number, NumberQuizMode mode, int askedCount, int correctCount)
    {
        var prompt = mode switch
        {
            NumberQuizMode.HebrewToNumberInput or NumberQuizMode.TransliterationToNumberInput => "Введите число от 0 до 99:",
            NumberQuizMode.HebrewToNumber or NumberQuizMode.TransliterationToNumber => "Какое это число?",
            _ => "Как называется это число на иврите?"
        };
        var value = mode switch
        {
            NumberQuizMode.HebrewToNumber or NumberQuizMode.HebrewToNumberInput => BidiText.Isolate(number.Hebrew),
            NumberQuizMode.TransliterationToNumber or NumberQuizMode.TransliterationToNumberInput => number.Transliteration,
            _ => number.Value.ToString(CultureInfo.InvariantCulture)
        };

        return string.Join("\n\n",
            BidiText.Line($"📊 Вопрос {(askedCount + 1).ToString(CultureInfo.InvariantCulture)} · верно {correctCount.ToString(CultureInfo.InvariantCulture)}"),
            BidiText.Line(prompt),
            BidiText.Line(value));
    }
}
