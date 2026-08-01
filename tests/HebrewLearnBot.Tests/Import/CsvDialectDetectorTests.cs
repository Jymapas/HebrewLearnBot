using HebrewLearnBot.Application.Import;

namespace HebrewLearnBot.Tests.Import;

public sealed class CsvDialectDetectorTests
{
    [Test]
    public void Detect_SemicolonHeader_ReturnsSemicolon()
    {
        var result = CsvDialectDetector.Detect("hebrew;translit;ru;topic");

        Assert.That(result, Is.EqualTo(';'));
    }

    [Test]
    public void Detect_TabHeader_ReturnsTab()
    {
        var result = CsvDialectDetector.Detect("hebrew\ttranslit\tru\ttopic");

        Assert.That(result, Is.EqualTo('\t'));
    }

    [Test]
    public void Detect_CommaHeader_ReturnsComma()
    {
        var result = CsvDialectDetector.Detect("hebrew,translit,ru,topic");

        Assert.That(result, Is.EqualTo(','));
    }

    [Test]
    public void Detect_DelimiterInsideQuotes_IsIgnored()
    {
        var result = CsvDialectDetector.Detect("hebrew,\"note: a;b;c;d\",translation");

        Assert.That(result, Is.EqualTo(','));
    }

    [Test]
    public void Detect_NoDelimiterFound_ThrowsCsvParseException()
    {
        var ex = Assert.Throws<CsvParseException>(() => CsvDialectDetector.Detect("hebrew translit ru topic"));

        Assert.That(ex.LineNumber, Is.EqualTo(1));
        Assert.That(ex.Message, Does.Contain("разделитель"));
    }
}
