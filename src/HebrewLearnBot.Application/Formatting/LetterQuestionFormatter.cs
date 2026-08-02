using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class LetterQuestionFormatter
{
    public static string BuildQuestionText(LetterCandidate correct, int askedCount, int correctCount)
    {
        var builder = new StringBuilder();

        builder.Append(BidiText.Line(
            $"📊 Вопрос {(askedCount + 1).ToString(CultureInfo.InvariantCulture)} · верно {correctCount.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line("Какая это буква?"));
        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line(BidiText.Isolate(correct.Glyph)));

        return builder.ToString();
    }

    public static string BuildOptionLabel(LetterCandidate candidate)
    {
        return candidate.Name;
    }
}
