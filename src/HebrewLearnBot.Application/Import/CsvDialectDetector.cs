namespace HebrewLearnBot.Application.Import;

public static class CsvDialectDetector
{
    private static readonly char[] CandidateDelimiters = ['\t', ';', ','];

    public static char Detect(string headerLine)
    {
        var counts = new Dictionary<char, int>
        {
            ['\t'] = 0,
            [';'] = 0,
            [','] = 0
        };

        var inQuotes = false;

        foreach (var c in headerLine)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (inQuotes)
            {
                continue;
            }

            if (counts.ContainsKey(c))
            {
                counts[c]++;
            }
        }

        var bestDelimiter = '\0';
        var bestCount = 0;

        foreach (var delimiter in CandidateDelimiters)
        {
            if (counts[delimiter] > bestCount)
            {
                bestCount = counts[delimiter];
                bestDelimiter = delimiter;
            }
        }

        if (bestCount == 0)
        {
            throw new CsvParseException(
                "Не удалось определить разделитель колонок. Используйте запятую, точку с запятой или табуляцию.",
                1);
        }

        return bestDelimiter;
    }
}
