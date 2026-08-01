using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class ImportReportFormatterTests
{
    [Test]
    public void Build_MoreErrorsThanLimit_TruncatesAndShowsRemainder()
    {
        var errors = Enumerable.Range(1, 25)
            .Select(line => new WordImportError(line, $"ошибка в строке {line}"))
            .ToList();
        var result = new WordImportResult(Added: 3, Updated: 0, Skipped: 0, Errors: errors);

        var text = ImportReportFormatter.Build(result);

        foreach (var error in errors.Take(ImportReportFormatter.MaxReportedErrors))
        {
            Assert.That(text, Does.Contain($"строка {error.LineNumber}:"));
        }

        Assert.That(text, Does.Contain("… и ещё 5"));
        Assert.That(text, Does.Not.Contain("строка 21:"));
    }

    [Test]
    public void Build_NothingImportedButErrorsPresent_ShowsWarningHeader()
    {
        var errors = new List<WordImportError> { new(1, "неверный формат") };
        var result = new WordImportResult(Added: 0, Updated: 0, Skipped: 0, Errors: errors);

        var text = ImportReportFormatter.Build(result);

        Assert.That(text, Does.Contain("⚠️ Импорт не добавил ни одного слова"));
    }

    [Test]
    public void Build_CleanImport_DoesNotShowErrorSection()
    {
        var result = new WordImportResult(Added: 5, Updated: 1, Skipped: 0, Errors: Array.Empty<WordImportError>());

        var text = ImportReportFormatter.Build(result);

        Assert.That(text, Does.Contain("✅ Импорт завершён"));
        Assert.That(text, Does.Not.Contain("Проблемные строки:"));
        Assert.That(text, Does.Not.Contain("Пропущено:"));
    }
}
