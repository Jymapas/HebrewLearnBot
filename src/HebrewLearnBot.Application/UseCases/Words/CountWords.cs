using HebrewLearnBot.Application.Abstractions.Persistence;

namespace HebrewLearnBot.Application.UseCases.Words;

public sealed class CountWords
{
    private readonly IWordRepository _wordRepository;

    public CountWords(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    public Task<int> ExecuteAsync(int? topicId, CancellationToken cancellationToken)
    {
        return _wordRepository.CountActiveAsync(topicId, cancellationToken);
    }
}
