using HebrewLearnBot.Application.Import;

namespace HebrewLearnBot.Tests.Import;

public sealed class CsvReaderTests
{
    [Test]
    public void Parse_BomAtStart_IsStripped()
    {
        var content = "﻿hebrew;translit\nא;alef";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0].Fields[0], Is.EqualTo("hebrew"));
    }

    [Test]
    public void Parse_QuotedFieldWithEmbeddedDelimiter_ParsesAsSingleField()
    {
        var content = "a;\"b;c\";d";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Fields, Is.EqualTo(new[] { "a", "b;c", "d" }));
    }

    [Test]
    public void Parse_EscapedDoubleQuote_UnescapesCorrectly()
    {
        var content = "a;\"She said \"\"hi\"\"\";c";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Fields[1], Is.EqualTo("She said \"hi\""));
    }

    [Test]
    public void Parse_QuotedFieldWithNewline_KeepsItInsideField()
    {
        var content = "a;\"line1\nline2\";c";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Fields[1], Is.EqualTo("line1\nline2"));
    }

    [Test]
    public void Parse_UnterminatedQuote_ReportsOpeningLineNumber()
    {
        var content = "a;b\n\"open;quote\nrest";

        var ex = Assert.Throws<CsvParseException>(() => CsvReader.Parse(content, ';'));

        Assert.That(ex.LineNumber, Is.EqualTo(2));
    }

    [Test]
    public void Parse_MixedCrLfAndLf_ParsesAllRows()
    {
        var content = "a;b\r\nc;d\ne;f";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(3));
        Assert.That(rows[0].LineNumber, Is.EqualTo(1));
        Assert.That(rows[1].LineNumber, Is.EqualTo(2));
        Assert.That(rows[2].LineNumber, Is.EqualTo(3));
        Assert.That(rows[0].Fields, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(rows[1].Fields, Is.EqualTo(new[] { "c", "d" }));
        Assert.That(rows[2].Fields, Is.EqualTo(new[] { "e", "f" }));
    }

    [Test]
    public void Parse_BlankLines_AreSkipped()
    {
        var content = "a;b\n\nc;d\n\n\ne;f";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(3));
        Assert.That(rows.Select(r => r.LineNumber), Is.EqualTo(new[] { 1, 3, 6 }));
    }

    [Test]
    public void Parse_UnquotedFieldWithSpaces_IsTrimmed()
    {
        var content = "  hello  ;  world  ";

        var rows = CsvReader.Parse(content, ';');

        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Fields, Is.EqualTo(new[] { "hello", "world" }));
    }
}
