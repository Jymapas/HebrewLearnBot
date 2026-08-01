using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Quiz;

internal sealed class CandidateBuilder
{
    private int _wordId = 1;
    private int _topicId = 1;
    private string _topicName = "Общая лексика";
    private PartOfSpeech _partOfSpeech = PartOfSpeech.Noun;
    private Gender? _gender;
    private string _hebrew = "שלום";
    private string _hebrewNormalized = "שלום";
    private string? _hebrewVocalized;
    private string _transliteration = "shalom";
    private string _translationRu = "привет";
    private string _translationNormalized = "привет";
    private bool _hasProgress;
    private int _correctCount;
    private int _wrongCount;
    private int _streak;
    private DateTime? _lastShownAtUtc;
    private bool? _lastAnswerCorrect;

    public CandidateBuilder WithWordId(int wordId)
    {
        _wordId = wordId;
        return this;
    }

    public CandidateBuilder WithTopicId(int topicId)
    {
        _topicId = topicId;
        return this;
    }

    public CandidateBuilder WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public CandidateBuilder WithPartOfSpeech(PartOfSpeech partOfSpeech)
    {
        _partOfSpeech = partOfSpeech;
        return this;
    }

    public CandidateBuilder WithGender(Gender? gender)
    {
        _gender = gender;
        return this;
    }

    public CandidateBuilder WithHebrew(string hebrew, string? normalized = null)
    {
        _hebrew = hebrew;
        _hebrewNormalized = normalized ?? hebrew;
        return this;
    }

    public CandidateBuilder WithHebrewVocalized(string? hebrewVocalized)
    {
        _hebrewVocalized = hebrewVocalized;
        return this;
    }

    public CandidateBuilder WithTransliteration(string transliteration)
    {
        _transliteration = transliteration;
        return this;
    }

    public CandidateBuilder WithTranslation(string translationRu, string? normalized = null)
    {
        _translationRu = translationRu;
        _translationNormalized = normalized ?? translationRu;
        return this;
    }

    public CandidateBuilder WithProgress(bool hasProgress = true)
    {
        _hasProgress = hasProgress;
        return this;
    }

    public CandidateBuilder WithCorrectCount(int correctCount)
    {
        _correctCount = correctCount;
        return this;
    }

    public CandidateBuilder WithWrongCount(int wrongCount)
    {
        _wrongCount = wrongCount;
        return this;
    }

    public CandidateBuilder WithStreak(int streak)
    {
        _streak = streak;
        return this;
    }

    public CandidateBuilder WithLastShownAtUtc(DateTime? lastShownAtUtc)
    {
        _lastShownAtUtc = lastShownAtUtc;
        return this;
    }

    public CandidateBuilder WithLastAnswerCorrect(bool? lastAnswerCorrect)
    {
        _lastAnswerCorrect = lastAnswerCorrect;
        return this;
    }

    public QuizCandidate Build()
    {
        return new QuizCandidate(
            _wordId,
            _topicId,
            _topicName,
            _partOfSpeech,
            _gender,
            _hebrew,
            _hebrewNormalized,
            _hebrewVocalized,
            _transliteration,
            _translationRu,
            _translationNormalized,
            _hasProgress,
            _correctCount,
            _wrongCount,
            _streak,
            _lastShownAtUtc,
            _lastAnswerCorrect);
    }
}
