using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeWordImportStore : IWordImportStore
{
    public WordUpsertOutcome OutcomeToReturn { get; set; } = new WordUpsertOutcome(0, 0);

    public bool ShouldThrow { get; set; }

    public int CallCount { get; private set; }

    public IReadOnlyList<WordImportRow>? LastRows { get; private set; }

    public Task<WordUpsertOutcome> UpsertAsync(IReadOnlyList<WordImportRow> rows, DateTime nowUtc, CancellationToken cancellationToken)
    {
        CallCount++;
        LastRows = rows;

        if (ShouldThrow)
        {
            throw new InvalidOperationException("Simulated store failure.");
        }

        return Task.FromResult(OutcomeToReturn);
    }
}
