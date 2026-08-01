using System.Text;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Application.Import;

public sealed record WordCsvParseOutcome(
    IReadOnlyList<WordImportRow> Rows,
    IReadOnlyList<WordImportError> Errors);

public sealed class WordCsvParser
{
    private const string EncodingErrorMessage =
        "Файл сохранён не в UTF-8, иврит в такой кодировке теряется. Пересохраните файл в UTF-8.";
    private const string NoDataRowsMessage = "Файл не содержит ни одной строки со словами.";

    private static readonly string[] MasculineAliases = ["м", "m", "муж", "мужской", "masculine"];
    private static readonly string[] FeminineAliases = ["ж", "f", "жен", "женский", "feminine"];

    private static readonly string[] NounAliases = ["сущ", "существительное", "noun"];
    private static readonly string[] VerbAliases = ["гл", "глагол", "verb"];
    private static readonly string[] AdjectiveAliases = ["прил", "прилагательное", "adj", "adjective"];
    private static readonly string[] AdverbAliases = ["нареч", "наречие", "adv", "adverb"];
    private static readonly string[] PhraseAliases = ["фраза", "phrase"];

    public WordCsvParseOutcome Parse(byte[] content, int maxRows)
    {
        var text = DecodeContent(content);

        if (text is null)
        {
            return new WordCsvParseOutcome([], [new WordImportError(1, EncodingErrorMessage)]);
        }

        try
        {
            return ParseText(text, maxRows);
        }
        catch (CsvParseException ex)
        {
            return new WordCsvParseOutcome([], [new WordImportError(ex.LineNumber, ex.Message)]);
        }
    }

    private static string? DecodeContent(byte[] content)
    {
        // Только строгий UTF-8. Однобайтовые кодировки вроде windows-1251 отдельно не пробуем:
        // колонка «иврит» обязательна, а буквы алефбета в такие кодировки не помещаются в принципе,
        // поэтому «успешный» разбор дал бы ноль валидных строк и гору однотипных ошибок вместо
        // одного внятного сообщения.
        var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        try
        {
            return strictUtf8.GetString(content);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }

    private static WordCsvParseOutcome ParseText(string text, int maxRows)
    {
        var headerLine = ExtractFirstNonEmptyLine(text);
        var delimiter = CsvDialectDetector.Detect(headerLine);

        var csvRows = CsvReader.Parse(text, delimiter);

        if (csvRows.Count == 0)
        {
            return new WordCsvParseOutcome([], [new WordImportError(1, NoDataRowsMessage)]);
        }

        var headerMap = CsvHeaderMap.Build(csvRows[0].Fields);
        var dataRows = csvRows.Skip(1).ToList();

        if (dataRows.Count == 0)
        {
            return new WordCsvParseOutcome([], [new WordImportError(1, NoDataRowsMessage)]);
        }

        var errors = new List<WordImportError>();

        if (dataRows.Count > maxRows)
        {
            errors.Add(new WordImportError(
                dataRows[maxRows].LineNumber,
                $"Файл содержит больше {maxRows} строк, лишние пропущены."));

            dataRows = dataRows.Take(maxRows).ToList();
        }

        var rows = new List<WordImportRow>();
        var rowIndexByKey = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var csvRow in dataRows)
        {
            var (row, error) = ValidateRow(csvRow, headerMap);

            if (error is not null)
            {
                errors.Add(error);
                continue;
            }

            var key = BuildDedupeKey(row!);

            if (rowIndexByKey.TryGetValue(key, out var existingIndex))
            {
                var previous = rows[existingIndex];
                errors.Add(new WordImportError(previous.LineNumber, $"дубликат, использована строка {row!.LineNumber}"));
                rows[existingIndex] = row;
            }
            else
            {
                rowIndexByKey[key] = rows.Count;
                rows.Add(row!);
            }
        }

        return new WordCsvParseOutcome(rows, errors);
    }

    private static string ExtractFirstNonEmptyLine(string text)
    {
        using var reader = new StringReader(text);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }

        return string.Empty;
    }

    private static (WordImportRow? Row, WordImportError? Error) ValidateRow(CsvRow csvRow, CsvHeaderMap headerMap)
    {
        var hebrewRaw = headerMap.GetValue(csvRow, CsvField.Hebrew) ?? string.Empty;

        if (hebrewRaw.Length == 0)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «иврит» пуста"));
        }

        if (!HebrewText.ContainsHebrewLetter(HebrewText.StripNiqqud(hebrewRaw)))
        {
            return (null, new WordImportError(csvRow.LineNumber, $"«{hebrewRaw}» — колонка «иврит» не содержит букв иврита"));
        }

        if (hebrewRaw.Length > 100)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «иврит» длиннее 100 символов"));
        }

        var vocalizedRaw = headerMap.GetValue(csvRow, CsvField.Vocalized);
        string? vocalized = null;

        if (!string.IsNullOrEmpty(vocalizedRaw))
        {
            if (vocalizedRaw.Length > 200)
            {
                return (null, new WordImportError(csvRow.LineNumber, "колонка «никуд» длиннее 200 символов"));
            }

            vocalized = vocalizedRaw;
        }

        var transliterationRaw = headerMap.GetValue(csvRow, CsvField.Transliteration) ?? string.Empty;

        if (transliterationRaw.Length == 0)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «транскрипция» пуста"));
        }

        if (!HebrewText.IsLatinTransliteration(transliterationRaw))
        {
            return (null, new WordImportError(csvRow.LineNumber, $"«{transliterationRaw}» — транскрипция содержит недопустимые символы"));
        }

        if (transliterationRaw.Length > 100)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «транскрипция» длиннее 100 символов"));
        }

        var translationRaw = headerMap.GetValue(csvRow, CsvField.Translation) ?? string.Empty;

        if (translationRaw.Length == 0)
        {
            return (null, new WordImportError(csvRow.LineNumber, "пустая колонка «перевод»"));
        }

        if (translationRaw.Length > 200)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «перевод» длиннее 200 символов"));
        }

        var topicRaw = headerMap.GetValue(csvRow, CsvField.Topic) ?? string.Empty;

        if (topicRaw.Length == 0)
        {
            return (null, new WordImportError(csvRow.LineNumber, "пустая колонка «тема»"));
        }

        if (topicRaw.Length > 100)
        {
            return (null, new WordImportError(csvRow.LineNumber, "колонка «тема» длиннее 100 символов"));
        }

        var genderRaw = headerMap.GetValue(csvRow, CsvField.Gender);
        Gender? gender = null;

        if (!string.IsNullOrEmpty(genderRaw))
        {
            gender = ParseGender(genderRaw);

            if (gender is null)
            {
                return (null, new WordImportError(csvRow.LineNumber, $"«{genderRaw}» — не удалось распознать род"));
            }
        }

        var partOfSpeechRaw = headerMap.GetValue(csvRow, CsvField.PartOfSpeech);
        var partOfSpeech = PartOfSpeech.Noun;

        if (!string.IsNullOrEmpty(partOfSpeechRaw))
        {
            var parsedPartOfSpeech = ParsePartOfSpeech(partOfSpeechRaw);

            if (parsedPartOfSpeech is null)
            {
                return (null, new WordImportError(csvRow.LineNumber, $"«{partOfSpeechRaw}» — не удалось распознать часть речи"));
            }

            partOfSpeech = parsedPartOfSpeech.Value;
        }

        if (gender is not null && partOfSpeech != PartOfSpeech.Noun)
        {
            return (null, new WordImportError(csvRow.LineNumber, "род указан не у существительного"));
        }

        var externalKeyRaw = headerMap.GetValue(csvRow, CsvField.ExternalKey);
        string? externalKey = null;

        if (!string.IsNullOrEmpty(externalKeyRaw))
        {
            if (externalKeyRaw.Length > 64)
            {
                return (null, new WordImportError(csvRow.LineNumber, "колонка «ключ» длиннее 64 символов"));
            }

            externalKey = externalKeyRaw;
        }

        var row = new WordImportRow(
            csvRow.LineNumber,
            hebrewRaw,
            vocalized,
            transliterationRaw,
            translationRaw,
            topicRaw,
            gender,
            partOfSpeech,
            externalKey);

        return (row, null);
    }

    private static Gender? ParseGender(string value)
    {
        var normalized = value.Trim();

        if (Array.Exists(MasculineAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return Gender.Masculine;
        }

        if (Array.Exists(FeminineAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return Gender.Feminine;
        }

        return null;
    }

    private static PartOfSpeech? ParsePartOfSpeech(string value)
    {
        var normalized = value.Trim();

        if (Array.Exists(NounAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return PartOfSpeech.Noun;
        }

        if (Array.Exists(VerbAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return PartOfSpeech.Verb;
        }

        if (Array.Exists(AdjectiveAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return PartOfSpeech.Adjective;
        }

        if (Array.Exists(AdverbAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return PartOfSpeech.Adverb;
        }

        if (Array.Exists(PhraseAliases, alias => string.Equals(alias, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return PartOfSpeech.Phrase;
        }

        return null;
    }

    private static string BuildDedupeKey(WordImportRow row)
    {
        if (!string.IsNullOrEmpty(row.ExternalKey))
        {
            return "key:" + row.ExternalKey;
        }

        var hebrewKey = TextNormalizer.NormalizeHebrew(row.Hebrew);
        var translationKey = TextNormalizer.NormalizeRussian(row.TranslationRu);

        return "pair:" + hebrewKey + " " + translationKey;
    }
}
