using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Alphabet;

namespace HebrewLearnBot.Application.UseCases.Alphabet;

public sealed class BuildLetterQuizSummary
{
    public LetterQuizSummary Execute(BuildLetterQuizSummaryCommand command)
    {
        if (command.WrongLetterIds.Count == 0)
        {
            return new LetterQuizSummary(command.AskedCount, command.CorrectCount, []);
        }

        // Сохраняем порядок первого появления, чтобы сводка совпадала с ходом квиза.
        var uniqueLetterIds = new List<int>();
        var seenLetterIds = new HashSet<int>();
        foreach (var letterId in command.WrongLetterIds)
        {
            if (seenLetterIds.Add(letterId))
            {
                uniqueLetterIds.Add(letterId);
            }
        }

        var briefs = uniqueLetterIds
            .Select(letterId => HebrewAlphabet.All.FirstOrDefault(letter => letter.Id == letterId))
            .Where(letter => letter is not null)
            .Select(letter => new LetterBrief(letter!.Id, letter.Glyph, letter.Name, letter.IsFinal))
            .ToList();

        return new LetterQuizSummary(command.AskedCount, command.CorrectCount, briefs);
    }
}

public sealed record BuildLetterQuizSummaryCommand(int AskedCount, int CorrectCount, IReadOnlyList<int> WrongLetterIds);
