using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IWordImportStore
{
    // Весь upsert выполняется одной транзакцией в Infrastructure: темы создаются на лету,
    // деактивированные слова воскрешаются.
    Task<WordUpsertOutcome> UpsertAsync(IReadOnlyList<WordImportRow> rows, DateTime nowUtc, CancellationToken cancellationToken);
}
