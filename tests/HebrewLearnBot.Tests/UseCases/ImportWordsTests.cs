using System.Text;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Import;
using HebrewLearnBot.Application.UseCases.Words;
using HebrewLearnBot.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class ImportWordsTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static byte[] Utf8(string text)
    {
        return new UTF8Encoding(false).GetBytes(text);
    }

    private static ImportWords CreateUseCase(FakeWordImportStore store)
    {
        return new ImportWords(new WordCsvParser(), store, new TestClock(NowUtc), NullLogger<ImportWords>.Instance);
    }

    [Test]
    public async Task ExecuteAsync_ValidCsv_ReturnsAddedAndUpdatedCounts()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;привет;приветствия\n" +
                      "בית;bayit;дом;дом";
        var store = new FakeWordImportStore { OutcomeToReturn = new WordUpsertOutcome(1, 1) };
        var useCase = CreateUseCase(store);
        var command = new ImportWordsCommand(1, Utf8(content), 100);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.Added, Is.EqualTo(1));
        Assert.That(result.Updated, Is.EqualTo(1));
        Assert.That(result.Skipped, Is.EqualTo(0));
        Assert.That(result.Errors, Is.Empty);
        Assert.That(store.CallCount, Is.EqualTo(1));
        Assert.That(store.LastRows, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_AllRowsInvalid_DoesNotTouchStore()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      ";shalom;привет;приветствия";
        var store = new FakeWordImportStore();
        var useCase = CreateUseCase(store);
        var command = new ImportWordsCommand(1, Utf8(content), 100);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.Added, Is.EqualTo(0));
        Assert.That(result.Updated, Is.EqualTo(0));
        Assert.That(result.Skipped, Is.EqualTo(1));
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(store.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_MixedValidAndInvalidRows_ReportsBothCountsAndErrors()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;привет;приветствия\n" +
                      ";shalom;привет;приветствия\n" +
                      "בית;bayit;дом;дом";
        var store = new FakeWordImportStore { OutcomeToReturn = new WordUpsertOutcome(2, 0) };
        var useCase = CreateUseCase(store);
        var command = new ImportWordsCommand(1, Utf8(content), 100);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.Added, Is.EqualTo(2));
        Assert.That(result.Updated, Is.EqualTo(0));
        Assert.That(result.Skipped, Is.EqualTo(1));
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(store.CallCount, Is.EqualTo(1));
        Assert.That(store.LastRows, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_NonUtf8Content_ReturnsEncodingErrorWithoutTouchingStore()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var content = "иврит;транскрипция;перевод;тема\n" +
                      "стол;stol;мебель;дом";
        var bytes = Encoding.GetEncoding(1251).GetBytes(content);
        var store = new FakeWordImportStore();
        var useCase = CreateUseCase(store);
        var command = new ImportWordsCommand(1, bytes, 100);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.Added, Is.EqualTo(0));
        Assert.That(result.Updated, Is.EqualTo(0));
        Assert.That(result.Skipped, Is.EqualTo(1));
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(result.Errors[0].Message, Does.Contain("UTF-8"));
        Assert.That(store.CallCount, Is.EqualTo(0));
    }
}
