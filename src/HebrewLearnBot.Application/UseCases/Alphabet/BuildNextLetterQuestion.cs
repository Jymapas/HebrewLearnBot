using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.Quiz;
using Microsoft.Extensions.Logging;

namespace HebrewLearnBot.Application.UseCases.Alphabet;

public sealed class BuildNextLetterQuestion
{
    private readonly ILetterCandidateReader _reader;
    private readonly LetterNextSelector _selector;
    private readonly LetterDistractorPicker _distractorPicker;
    private readonly WeightedPicker _weightedPicker;
    private readonly IClock _clock;
    private readonly ILogger<BuildNextLetterQuestion> _logger;

    public BuildNextLetterQuestion(
        ILetterCandidateReader reader,
        LetterNextSelector selector,
        LetterDistractorPicker distractorPicker,
        WeightedPicker weightedPicker,
        IClock clock,
        ILogger<BuildNextLetterQuestion> logger)
    {
        _reader = reader;
        _selector = selector;
        _distractorPicker = distractorPicker;
        _weightedPicker = weightedPicker;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LetterQuizQuestion?> ExecuteAsync(BuildNextLetterQuestionCommand command, CancellationToken cancellationToken)
    {
        var candidates = await _reader.LoadCandidatesAsync(command.UserId, cancellationToken);

        if (candidates.Count < QuizConstants.OptionCount)
        {
            _logger.LogInformation(
                "Not enough letter candidates for user {UserId} to build a question: {CandidateCount}",
                command.UserId,
                candidates.Count);
            return null;
        }

        var correct = _selector.Select(candidates, command.RecentLetterIds, _clock.UtcNow);

        if (correct is null)
        {
            return null;
        }

        var distractors = _distractorPicker.Pick(correct, candidates, QuizConstants.OptionCount - 1);

        var optionCandidates = new List<LetterCandidate>(1 + distractors.Count) { correct };
        optionCandidates.AddRange(distractors);

        _weightedPicker.Shuffle(optionCandidates);

        var correctIndex = optionCandidates.FindIndex(candidate => candidate.LetterId == correct.LetterId);

        var options = optionCandidates
            .Select(candidate => new LetterQuizOption(candidate.LetterId, LetterQuestionFormatter.BuildOptionLabel(candidate)))
            .ToList();

        var questionText = LetterQuestionFormatter.BuildQuestionText(correct, command.AskedCount, command.CorrectCount);

        return new LetterQuizQuestion(ToLetterBrief(correct), questionText, options, correctIndex);
    }

    private static LetterBrief ToLetterBrief(LetterCandidate candidate)
    {
        return new LetterBrief(candidate.LetterId, candidate.Glyph, candidate.Name, candidate.IsFinal);
    }
}

public sealed record BuildNextLetterQuestionCommand(
    long UserId,
    IReadOnlyList<int> RecentLetterIds,
    int AskedCount,
    int CorrectCount);
