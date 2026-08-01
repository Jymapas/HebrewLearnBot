namespace HebrewLearnBot.Application.Import;

public sealed class CsvParseException : Exception
{
    public CsvParseException(string message, int lineNumber)
        : base(message)
    {
        LineNumber = lineNumber;
    }

    public int LineNumber { get; }
}
