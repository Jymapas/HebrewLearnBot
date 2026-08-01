namespace HebrewLearnBot.Application.Import;

public enum CsvField
{
    Hebrew,
    Vocalized,
    Transliteration,
    Translation,
    Topic,
    Gender,
    PartOfSpeech,
    ExternalKey
}

public sealed class CsvHeaderMap
{
    private static readonly IReadOnlyDictionary<CsvField, string[]> Aliases = new Dictionary<CsvField, string[]>
    {
        [CsvField.Hebrew] = ["hebrew", "he", "иврит"],
        [CsvField.Vocalized] = ["vocalized", "hebrew_vocalized", "никуд", "огласовки"],
        [CsvField.Transliteration] = ["translit", "transliteration", "транскрипция"],
        [CsvField.Translation] = ["ru", "translation", "translation_ru", "перевод"],
        [CsvField.Topic] = ["topic", "category", "тема"],
        [CsvField.Gender] = ["gender", "род"],
        [CsvField.PartOfSpeech] = ["pos", "part_of_speech", "часть_речи", "часть речи"],
        [CsvField.ExternalKey] = ["key", "external_key"]
    };

    private static readonly CsvField[] RequiredFields =
    [
        CsvField.Hebrew,
        CsvField.Transliteration,
        CsvField.Translation,
        CsvField.Topic
    ];

    private static readonly IReadOnlyDictionary<CsvField, string> DisplayNames = new Dictionary<CsvField, string>
    {
        [CsvField.Hebrew] = "иврит",
        [CsvField.Transliteration] = "транскрипция",
        [CsvField.Translation] = "перевод",
        [CsvField.Topic] = "тема"
    };

    private readonly IReadOnlyDictionary<CsvField, int> _columnIndexes;

    private CsvHeaderMap(IReadOnlyDictionary<CsvField, int> columnIndexes)
    {
        _columnIndexes = columnIndexes;
    }

    public static CsvHeaderMap Build(IReadOnlyList<string> headerFields)
    {
        var columnIndexes = new Dictionary<CsvField, int>();

        for (var i = 0; i < headerFields.Count; i++)
        {
            var name = headerFields[i].Trim();

            foreach (var pair in Aliases)
            {
                if (columnIndexes.ContainsKey(pair.Key))
                {
                    continue;
                }

                if (Array.Exists(pair.Value, alias => string.Equals(alias, name, StringComparison.OrdinalIgnoreCase)))
                {
                    columnIndexes[pair.Key] = i;
                }
            }
        }

        foreach (var field in RequiredFields)
        {
            if (!columnIndexes.ContainsKey(field))
            {
                throw new CsvParseException($"Отсутствует обязательная колонка «{DisplayNames[field]}».", 1);
            }
        }

        return new CsvHeaderMap(columnIndexes);
    }

    public string? GetValue(CsvRow row, CsvField field)
    {
        if (!_columnIndexes.TryGetValue(field, out var index))
        {
            return null;
        }

        if (index >= row.Fields.Count)
        {
            return null;
        }

        return row.Fields[index];
    }
}
