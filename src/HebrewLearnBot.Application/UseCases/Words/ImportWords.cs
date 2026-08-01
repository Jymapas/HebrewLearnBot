using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Import;
using Microsoft.Extensions.Logging;

namespace HebrewLearnBot.Application.UseCases.Words;

public sealed class ImportWords
{
    private readonly WordCsvParser _parser;
    private readonly IWordImportStore _store;
    private readonly IClock _clock;
    private readonly ILogger<ImportWords> _logger;

    public ImportWords(WordCsvParser parser, IWordImportStore store, IClock clock, ILogger<ImportWords> logger)
    {
        _parser = parser;
        _store = store;
        _clock = clock;
        _logger = logger;
    }

    public async Task<WordImportResult> ExecuteAsync(ImportWordsCommand command, CancellationToken cancellationToken)
    {
        var outcome = _parser.Parse(command.Content, command.MaxRows);

        if (outcome.Rows.Count == 0)
        {
            var emptyResult = new WordImportResult(0, 0, outcome.Errors.Count, outcome.Errors);

            _logger.LogInformation(
                "Import by user {UserId}: added {Added}, updated {Updated}, errors {Errors}",
                command.UserId,
                emptyResult.Added,
                emptyResult.Updated,
                emptyResult.Errors.Count);

            return emptyResult;
        }

        var upsert = await _store.UpsertAsync(outcome.Rows, _clock.UtcNow, cancellationToken);

        var result = new WordImportResult(upsert.Added, upsert.Updated, outcome.Errors.Count, outcome.Errors);

        _logger.LogInformation(
            "Import by user {UserId}: added {Added}, updated {Updated}, errors {Errors}",
            command.UserId,
            result.Added,
            result.Updated,
            result.Errors.Count);

        return result;
    }
}

public sealed record ImportWordsCommand(long UserId, byte[] Content, int MaxRows);
