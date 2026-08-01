using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HebrewLearnBot.Application.UseCases.Quiz;

public sealed class BuildNextQuestion
{
    private readonly IQuizCandidateReader _reader;
    private readonly NextWordSelector _selector;
    private readonly DistractorPicker _distractorPicker;
    private readonly WeightedPicker _weightedPicker;
    private readonly IClock _clock;
    private readonly QuizPoolOptions _poolOptions;
    private readonly ILogger<BuildNextQuestion> _logger;

    public BuildNextQuestion(
        IQuizCandidateReader reader,
        NextWordSelector selector,
        DistractorPicker distractorPicker,
        WeightedPicker weightedPicker,
        IClock clock,
        QuizPoolOptions poolOptions,
        ILogger<BuildNextQuestion> logger)
    {
        _reader = reader;
        _selector = selector;
        _distractorPicker = distractorPicker;
        _weightedPicker = weightedPicker;
        _clock = clock;
        _poolOptions = poolOptions;
        _logger = logger;
    }

    public async Task<QuizQuestion?> ExecuteAsync(BuildNextQuestionCommand command, CancellationToken cancellationToken)
    {
        var candidates = await _reader.LoadCandidatesAsync(
            command.UserId,
            command.TopicId,
            _poolOptions.CandidatePoolLimit,
            cancellationToken);

        if (candidates.Count < QuizConstants.MinimumWordsForQuiz)
        {
            _logger.LogInformation(
                "Not enough candidates for user {UserId} to build a quiz question: {CandidateCount}",
                command.UserId,
                candidates.Count);
            return null;
        }

        var correct = _selector.Select(candidates, command.RecentWordIds, _clock.UtcNow);

        if (correct is null)
        {
            return null;
        }

        var distractors = _distractorPicker.Pick(correct, candidates, command.Mode, QuizConstants.OptionCount - 1);

        var optionCandidates = new List<QuizCandidate>(1 + distractors.Count) { correct };
        optionCandidates.AddRange(distractors);

        _weightedPicker.Shuffle(optionCandidates);

        var correctIndex = optionCandidates.FindIndex(candidate => candidate.WordId == correct.WordId);

        var options = optionCandidates
            .Select(candidate => new QuizOption(candidate.WordId, QuestionFormatter.BuildOptionLabel(command.Mode, candidate)))
            .ToList();

        var questionText = QuestionFormatter.BuildQuestionText(command.Mode, correct, command.AskedCount, command.CorrectCount);

        return new QuizQuestion(command.Mode, ToWordBrief(correct), questionText, options, correctIndex);
    }

    private static WordBrief ToWordBrief(QuizCandidate candidate)
    {
        return new WordBrief(
            candidate.WordId,
            candidate.Hebrew,
            candidate.HebrewVocalized,
            candidate.Transliteration,
            candidate.TranslationRu,
            candidate.Gender,
            candidate.TopicName);
    }
}

public sealed record BuildNextQuestionCommand(
    long UserId,
    QuizMode Mode,
    int? TopicId,
    IReadOnlyList<int> RecentWordIds,
    int AskedCount,
    int CorrectCount);
