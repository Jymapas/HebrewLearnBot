using HebrewLearnBot.Application.Formatting;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class BidiTextTests
{
    [Test]
    public void Isolate_HebrewFragment_WrapsWithLeftToRightMarks()
    {
        var result = BidiText.Isolate("שולחן");

        Assert.That(result, Is.EqualTo($"{BidiText.LeftToRightMark}שולחן{BidiText.LeftToRightMark}"));
    }

    [Test]
    public void Isolate_TextWithoutHebrew_ReturnsUnchanged()
    {
        var result = BidiText.Isolate("shulkhan");

        Assert.That(result, Is.EqualTo("shulkhan"));
    }

    [Test]
    public void Line_AlreadyPrefixed_DoesNotDuplicateMark()
    {
        var alreadyPrefixed = $"{BidiText.LeftToRightMark}Как переводится?";

        var result = BidiText.Line(alreadyPrefixed);

        Assert.That(result, Is.EqualTo(alreadyPrefixed));
        Assert.That(result.Count(c => c == BidiText.LeftToRightMark), Is.EqualTo(1));
    }

    [Test]
    public void Line_NullOrEmpty_ReturnsEmpty()
    {
        Assert.That(BidiText.Line(null), Is.EqualTo(string.Empty));
        Assert.That(BidiText.Line(string.Empty), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ForButton_LongLabel_TruncatesAndKeepsLeadingMark()
    {
        var longLabel = new string('א', 100);

        var result = BidiText.ForButton(longLabel);

        Assert.That(result[0], Is.EqualTo(BidiText.LeftToRightMark));
        Assert.That(result, Does.Contain("…"));
        Assert.That(result.Length, Is.LessThan(longLabel.Length));
    }

    [Test]
    public void Join_MixedParts_IsolatesOnlyHebrewOnes()
    {
        var result = BidiText.Join(" — ", "שולחן", "table");

        Assert.That(result, Is.EqualTo($"{BidiText.Isolate("שולחן")} — table"));
        Assert.That(result.IndexOf($"{BidiText.LeftToRightMark}table", StringComparison.Ordinal), Is.EqualTo(-1));
    }
}
