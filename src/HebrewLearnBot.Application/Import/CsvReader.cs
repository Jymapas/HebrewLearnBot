using System.Text;

namespace HebrewLearnBot.Application.Import;

public sealed record CsvRow(int LineNumber, IReadOnlyList<string> Fields);

public static class CsvReader
{
    public static IReadOnlyList<CsvRow> Parse(string content, char delimiter)
    {
        var rows = new List<CsvRow>();

        if (string.IsNullOrEmpty(content))
        {
            return rows;
        }

        var text = content[0] == '﻿' ? content[1..] : content;
        var length = text.Length;

        var lineNumber = 1;
        var rowStartLine = 1;
        var rowStartIndex = 0;

        var fields = new List<string>();
        var field = new StringBuilder();
        var fieldIsQuoted = false;
        var inQuotes = false;
        var quoteOpenLine = 0;

        var i = 0;

        while (i < length)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                        continue;
                    }

                    inQuotes = false;
                    i++;
                    continue;
                }

                if (c == '\r' && i + 1 < length && text[i + 1] == '\n')
                {
                    field.Append('\r').Append('\n');
                    lineNumber++;
                    i += 2;
                    continue;
                }

                if (c is '\n' or '\r')
                {
                    field.Append(c);
                    lineNumber++;
                    i++;
                    continue;
                }

                field.Append(c);
                i++;
                continue;
            }

            if (c == '"' && field.Length == 0 && !fieldIsQuoted)
            {
                inQuotes = true;
                fieldIsQuoted = true;
                quoteOpenLine = lineNumber;
                i++;
                continue;
            }

            if (c == delimiter)
            {
                fields.Add(FinishField(field, fieldIsQuoted));
                fieldIsQuoted = false;
                i++;
                continue;
            }

            if (c == '\r' && i + 1 < length && text[i + 1] == '\n')
            {
                AppendRow(rows, fields, field, fieldIsQuoted, rowStartIndex, i, rowStartLine);
                fields = new List<string>();
                fieldIsQuoted = false;
                lineNumber++;
                i += 2;
                rowStartIndex = i;
                rowStartLine = lineNumber;
                continue;
            }

            if (c is '\n' or '\r')
            {
                AppendRow(rows, fields, field, fieldIsQuoted, rowStartIndex, i, rowStartLine);
                fields = new List<string>();
                fieldIsQuoted = false;
                lineNumber++;
                i++;
                rowStartIndex = i;
                rowStartLine = lineNumber;
                continue;
            }

            field.Append(c);
            i++;
        }

        if (inQuotes)
        {
            throw new CsvParseException("Незакрытая кавычка.", quoteOpenLine);
        }

        AppendRow(rows, fields, field, fieldIsQuoted, rowStartIndex, i, rowStartLine);

        return rows;
    }

    private static void AppendRow(
        List<CsvRow> rows,
        List<string> fields,
        StringBuilder field,
        bool fieldIsQuoted,
        int rowStartIndex,
        int rowEndIndex,
        int rowStartLine)
    {
        if (fields.Count == 0 && rowEndIndex == rowStartIndex)
        {
            // Полностью пустая строка исходного файла — пропускаем её.
            return;
        }

        fields.Add(FinishField(field, fieldIsQuoted));
        rows.Add(new CsvRow(rowStartLine, fields));
    }

    private static string FinishField(StringBuilder field, bool fieldIsQuoted)
    {
        var value = field.ToString();
        field.Clear();

        return fieldIsQuoted ? value : value.Trim();
    }
}
