using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.UseCases.Quiz;

public sealed class BuildQuizSummary
{
    private readonly IWordRepository _wordRepository;

    public BuildQuizSummary(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    public async Task<QuizSummary> ExecuteAsync(BuildQuizSummaryCommand command, CancellationToken cancellationToken)
    {
        if (command.WrongWordIds.Count == 0)
        {
            return new QuizSummary(command.AskedCount, command.CorrectCount, []);
        }

        // Сохраняем порядок первого появления, чтобы сводка совпадала с ходом квиза.
        var uniqueWordIds = new List<int>();
        var seenWordIds = new HashSet<int>();
        foreach (var wordId in command.WrongWordIds)
        {
            if (seenWordIds.Add(wordId))
            {
                uniqueWordIds.Add(wordId);
            }
        }

        var briefs = await _wordRepository.GetBriefsByIdsAsync(uniqueWordIds, cancellationToken);

        return new QuizSummary(command.AskedCount, command.CorrectCount, briefs);
    }
}

public sealed record BuildQuizSummaryCommand(int AskedCount, int CorrectCount, IReadOnlyList<int> WrongWordIds);
