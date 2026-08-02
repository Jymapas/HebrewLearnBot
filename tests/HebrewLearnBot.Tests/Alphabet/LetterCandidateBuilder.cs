using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Alphabet;

internal sealed class LetterCandidateBuilder
{
    private int _letterId = 1;
    private string _glyph = "א";
    private string _name = "Алеф";
    private bool _isFinal;
    private bool _hasProgress;
    private int _correctCount;
    private int _wrongCount;
    private int _streak;
    private DateTime? _lastShownAtUtc;
    private bool? _lastAnswerCorrect;

    public LetterCandidateBuilder WithLetterId(int letterId)
    {
        _letterId = letterId;
        return this;
    }

    public LetterCandidateBuilder WithGlyph(string glyph)
    {
        _glyph = glyph;
        return this;
    }

    public LetterCandidateBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public LetterCandidateBuilder WithIsFinal(bool isFinal)
    {
        _isFinal = isFinal;
        return this;
    }

    public LetterCandidateBuilder WithProgress(bool hasProgress = true)
    {
        _hasProgress = hasProgress;
        return this;
    }

    public LetterCandidateBuilder WithCorrectCount(int correctCount)
    {
        _correctCount = correctCount;
        return this;
    }

    public LetterCandidateBuilder WithWrongCount(int wrongCount)
    {
        _wrongCount = wrongCount;
        return this;
    }

    public LetterCandidateBuilder WithStreak(int streak)
    {
        _streak = streak;
        return this;
    }

    public LetterCandidateBuilder WithLastShownAtUtc(DateTime? lastShownAtUtc)
    {
        _lastShownAtUtc = lastShownAtUtc;
        return this;
    }

    public LetterCandidateBuilder WithLastAnswerCorrect(bool? lastAnswerCorrect)
    {
        _lastAnswerCorrect = lastAnswerCorrect;
        return this;
    }

    public LetterCandidate Build()
    {
        return new LetterCandidate(
            _letterId,
            _glyph,
            _name,
            _isFinal,
            _hasProgress,
            _correctCount,
            _wrongCount,
            _streak,
            _lastShownAtUtc,
            _lastAnswerCorrect);
    }
}
