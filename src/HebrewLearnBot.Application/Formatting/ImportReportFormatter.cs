using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class ImportReportFormatter
{
    public const int MaxReportedErrors = 20;

    public static string Build(WordImportResult result)
    {
        var builder = new StringBuilder();

        var nothingImported = result.Added == 0 && result.Updated == 0 && result.Errors.Count > 0;
        var header = nothingImported ? "⚠️ Импорт не добавил ни одного слова" : "✅ Импорт завершён";

        builder.Append(BidiText.Line(header));
        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line($"Добавлено: {result.Added.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append(BidiText.Line($"Обновлено: {result.Updated.ToString(CultureInfo.InvariantCulture)}"));

        if (result.Skipped > 0)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"Пропущено: {result.Skipped.ToString(CultureInfo.InvariantCulture)}"));
        }

        builder.Append('\n');
        builder.Append(BidiText.Line($"Ошибок: {result.Errors.Count.ToString(CultureInfo.InvariantCulture)}"));

        if (result.Errors.Count == 0)
        {
            return builder.ToString();
        }

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line("Проблемные строки:"));

        var reportedCount = Math.Min(result.Errors.Count, MaxReportedErrors);

        for (var i = 0; i < reportedCount; i++)
        {
            var error = result.Errors[i];
            builder.Append('\n');
            builder.Append(BidiText.Line(
                $"• строка {error.LineNumber.ToString(CultureInfo.InvariantCulture)}: {error.Message}"));
        }

        var remaining = result.Errors.Count - reportedCount;

        if (remaining > 0)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"… и ещё {remaining.ToString(CultureInfo.InvariantCulture)}"));
        }

        return builder.ToString();
    }
}
