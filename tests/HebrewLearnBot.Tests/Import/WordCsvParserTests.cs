using System.Text;
using HebrewLearnBot.Application.Import;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Import;

public sealed class WordCsvParserTests
{
    private static byte[] Utf8(string text)
    {
        return new UTF8Encoding(false).GetBytes(text);
    }

    [Test]
    public void Parse_ValidSemicolonFile_ReturnsAllRows()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;привет;приветствия\n" +
                      "בית;bayit;дом;дом";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Rows, Has.Count.EqualTo(2));

        Assert.That(outcome.Rows[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Rows[0].Hebrew, Is.EqualTo("שלום"));
        Assert.That(outcome.Rows[0].Transliteration, Is.EqualTo("shalom"));
        Assert.That(outcome.Rows[0].TranslationRu, Is.EqualTo("привет"));
        Assert.That(outcome.Rows[0].TopicName, Is.EqualTo("приветствия"));
        Assert.That(outcome.Rows[0].Gender, Is.Null);
        Assert.That(outcome.Rows[0].PartOfSpeech, Is.EqualTo(PartOfSpeech.Noun));
        Assert.That(outcome.Rows[0].ExternalKey, Is.Null);

        Assert.That(outcome.Rows[1].LineNumber, Is.EqualTo(3));
        Assert.That(outcome.Rows[1].Hebrew, Is.EqualTo("בית"));
    }

    [Test]
    public void Parse_RussianColumnNames_MapsToFields()
    {
        var content = "иврит;транскрипция;перевод;тема\n" +
                      "שלום;shalom;привет;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Rows, Has.Count.EqualTo(1));
        Assert.That(outcome.Rows[0].Hebrew, Is.EqualTo("שלום"));
        Assert.That(outcome.Rows[0].Transliteration, Is.EqualTo("shalom"));
        Assert.That(outcome.Rows[0].TranslationRu, Is.EqualTo("привет"));
        Assert.That(outcome.Rows[0].TopicName, Is.EqualTo("приветствия"));
    }

    [Test]
    public void Parse_ColumnsInDifferentOrder_MapsCorrectly()
    {
        var content = "ru;hebrew;topic;translit\n" +
                      "привет;שלום;приветствия;shalom";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Rows, Has.Count.EqualTo(1));
        Assert.That(outcome.Rows[0].Hebrew, Is.EqualTo("שלום"));
        Assert.That(outcome.Rows[0].Transliteration, Is.EqualTo("shalom"));
        Assert.That(outcome.Rows[0].TranslationRu, Is.EqualTo("привет"));
        Assert.That(outcome.Rows[0].TopicName, Is.EqualTo("приветствия"));
    }

    [Test]
    public void Parse_MissingRequiredColumn_ReturnsHeaderError()
    {
        var content = "hebrew;translit;ru\n" +
                      "שלום;shalom;привет";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(1));
        Assert.That(outcome.Errors[0].Message, Does.Contain("тема"));
    }

    [Test]
    public void Parse_EmptyTranslation_ReturnsRowErrorWithLineNumber()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("пустая колонка «перевод»"));
    }

    [Test]
    public void Parse_HebrewColumnWithoutHebrewLetters_ReturnsRowError()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "shalom;shalom;привет;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Does.Contain("не содержит букв иврита"));
    }

    [Test]
    public void Parse_TransliterationWithCyrillic_ReturnsRowError()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;шалом;привет;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Does.Contain("недопустимые символы"));
    }

    [Test]
    public void Parse_GenderAliases_ParseToExpectedEnum()
    {
        var content = "hebrew;translit;ru;topic;gender\n" +
                      "בית;bayit;дом;дом;м\n" +
                      "דלת;delet;дверь;дом;ж\n" +
                      "אישה;isha;женщина;люди;f\n" +
                      "עוגה;uga;торт;еда;женский";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Rows, Has.Count.EqualTo(4));
        Assert.That(outcome.Rows[0].Gender, Is.EqualTo(Gender.Masculine));
        Assert.That(outcome.Rows[1].Gender, Is.EqualTo(Gender.Feminine));
        Assert.That(outcome.Rows[2].Gender, Is.EqualTo(Gender.Feminine));
        Assert.That(outcome.Rows[3].Gender, Is.EqualTo(Gender.Feminine));
    }

    [Test]
    public void Parse_GenderOnVerb_ReturnsRowError()
    {
        var content = "hebrew;translit;ru;topic;gender;pos\n" +
                      "אכל;akhal;есть;глаголы;м;verb";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("род указан не у существительного"));
    }

    [Test]
    public void Parse_PartOfSpeechDefaultsToNoun_WhenColumnAbsent()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;привет;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Rows, Has.Count.EqualTo(1));
        Assert.That(outcome.Rows[0].PartOfSpeech, Is.EqualTo(PartOfSpeech.Noun));
    }

    [Test]
    public void Parse_DuplicateExternalKey_LastRowWinsAndReportsPrevious()
    {
        var content = "hebrew;translit;ru;topic;key\n" +
                      "שלום;shalom;привет;приветствия;k1\n" +
                      "שלום;shaloom;хай;приветствия;k1";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Has.Count.EqualTo(1));
        Assert.That(outcome.Rows[0].LineNumber, Is.EqualTo(3));
        Assert.That(outcome.Rows[0].Transliteration, Is.EqualTo("shaloom"));

        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("дубликат, использована строка 3"));
    }

    [Test]
    public void Parse_DuplicateHebrewAndTranslation_LastRowWins()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;Привет;приветствия\n" +
                      "שלום;shaloom;привет;приветствия";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Has.Count.EqualTo(1));
        Assert.That(outcome.Rows[0].LineNumber, Is.EqualTo(3));
        Assert.That(outcome.Rows[0].Transliteration, Is.EqualTo("shaloom"));

        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("дубликат, использована строка 3"));
    }

    [Test]
    public void Parse_RowLimitExceeded_TruncatesAndReportsError()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "בית;bayit;дом;дом\n" +
                      "דלת;delet;дверь;дом\n" +
                      "חלון;chalon;окно;дом";

        var outcome = new WordCsvParser().Parse(Utf8(content), 2);

        Assert.That(outcome.Rows, Has.Count.EqualTo(2));
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(4));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("Файл содержит больше 2 строк, лишние пропущены."));
    }

    [Test]
    public void Parse_NonUtf8Content_ReturnsSingleEncodingError()
    {
        // Буквы иврита не помещаются в windows-1251, поэтому такой файл не может дать
        // ни одной валидной строки. Пользователь должен получить одну внятную ошибку
        // про кодировку, а не по ошибке на каждую строку файла.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var content = "иврит;транскрипция;перевод;тема\n" +
                      "стол;stol;мебель;дом";

        var bytes = Encoding.GetEncoding(1251).GetBytes(content);

        var outcome = new WordCsvParser().Parse(bytes, 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(1));
        Assert.That(
            outcome.Errors[0].Message,
            Is.EqualTo("Файл сохранён не в UTF-8, иврит в такой кодировке теряется. Пересохраните файл в UTF-8."));
    }

    [Test]
    public void Parse_AsciiOnlyContent_IsStillDecodedAsUtf8()
    {
        // ASCII — подмножество UTF-8, такие файлы должны читаться и доходить до построчной
        // валидации, а не отвергаться как «не UTF-8».
        var content = "hebrew;translit;ru;topic\n" +
                      "table;shulkhan;stol;home";

        var outcome = new WordCsvParser().Parse(Encoding.ASCII.GetBytes(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Errors[0].Message, Does.Contain("не содержит букв иврита"));
    }

    [Test]
    public void Parse_HeaderOnly_ReturnsNoRowsError()
    {
        var content = "hebrew;translit;ru;topic";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Is.Empty);
        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(1));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("Файл не содержит ни одной строки со словами."));
    }

    [Test]
    public void Parse_ValidAndInvalidRowsMixed_ReturnsBothRowsAndErrors()
    {
        var content = "hebrew;translit;ru;topic\n" +
                      "שלום;shalom;привет;приветствия\n" +
                      ";shalom;привет;приветствия\n" +
                      "בית;bayit;дом;дом";

        var outcome = new WordCsvParser().Parse(Utf8(content), 100);

        Assert.That(outcome.Rows, Has.Count.EqualTo(2));
        Assert.That(outcome.Rows[0].LineNumber, Is.EqualTo(2));
        Assert.That(outcome.Rows[1].LineNumber, Is.EqualTo(4));

        Assert.That(outcome.Errors, Has.Count.EqualTo(1));
        Assert.That(outcome.Errors[0].LineNumber, Is.EqualTo(3));
        Assert.That(outcome.Errors[0].Message, Is.EqualTo("колонка «иврит» пуста"));
    }
}
