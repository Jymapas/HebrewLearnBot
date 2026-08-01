using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace HebrewLearnBot.Tests.Quiz;

public sealed class DistractorPickerTests
{
    [Test]
    public void Pick_SameTopicHasEnoughWords_UsesOnlySameTopic()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithPartOfSpeech(PartOfSpeech.Noun)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>();
        for (var i = 2; i <= 6; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(1)
                .WithPartOfSpeech(i % 2 == 0 ? PartOfSpeech.Noun : PartOfSpeech.Verb)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        for (var i = 7; i <= 11; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(2)
                .WithPartOfSpeech(PartOfSpeech.Noun)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 1), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.All(c => c.TopicId == 1), Is.True);
    }

    [Test]
    public void Pick_TopicTooSmall_FallsBackToOtherTopics()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithPartOfSpeech(PartOfSpeech.Noun)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>
        {
            new CandidateBuilder()
                .WithWordId(2)
                .WithTopicId(1)
                .WithPartOfSpeech(PartOfSpeech.Noun)
                .WithHebrew("שתיים")
                .WithTranslation("два")
                .Build()
        };
        for (var i = 3; i <= 7; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(2)
                .WithPartOfSpeech(PartOfSpeech.Verb)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 2), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Any(c => c.TopicId != 1), Is.True);
    }

    [Test]
    public void Pick_RuAnswerMode_NeverReturnsDuplicateTranslation()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>
        {
            new CandidateBuilder().WithWordId(2).WithTopicId(1).WithHebrew("ספר").WithTranslation("книга").Build(),
            new CandidateBuilder().WithWordId(3).WithTopicId(1).WithHebrew("חוברת").WithTranslation("книга").Build(),
            new CandidateBuilder().WithWordId(4).WithTopicId(1).WithHebrew("עט").WithTranslation("ручка").Build(),
            new CandidateBuilder().WithWordId(5).WithTopicId(1).WithHebrew("שולחן").WithTranslation("стол").Build()
        };

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 3), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        var translations = result.Select(c => c.TranslationNormalized).ToList();
        Assert.That(translations.Distinct().Count(), Is.EqualTo(translations.Count));
    }

    [Test]
    public void Pick_HeAnswerMode_NeverReturnsDuplicateHebrew()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>
        {
            new CandidateBuilder().WithWordId(2).WithTopicId(1).WithHebrew("בית").WithTranslation("дом").Build(),
            new CandidateBuilder().WithWordId(3).WithTopicId(1).WithHebrew("בית").WithTranslation("здание").Build(),
            new CandidateBuilder().WithWordId(4).WithTopicId(1).WithHebrew("חלון").WithTranslation("окно").Build(),
            new CandidateBuilder().WithWordId(5).WithTopicId(1).WithHebrew("דלת").WithTranslation("дверь").Build()
        };

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 4), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.RuToHe, 3);

        var hebrewKeys = result.Select(c => c.HebrewNormalized).ToList();
        Assert.That(hebrewKeys.Distinct().Count(), Is.EqualTo(hebrewKeys.Count));
    }

    [Test]
    public void Pick_NeverIncludesCorrectWord()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate> { correct };
        for (var i = 2; i <= 6; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(1)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 6), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result.Any(c => c.WordId == correct.WordId), Is.False);
    }

    [Test]
    public void Pick_OnlyTwoUsableWords_ReturnsFewerThanRequested()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>
        {
            new CandidateBuilder().WithWordId(2).WithTopicId(1).WithHebrew("שתיים").WithTranslation("два").Build(),
            new CandidateBuilder().WithWordId(3).WithTopicId(1).WithHebrew("שלוש").WithTranslation("три").Build()
        };

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 8), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Count, Is.LessThan(3));
    }

    [Test]
    public void Pick_ExactlyFourWordsInPool_ReturnsThreeDistinctDistractors()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>
        {
            correct,
            new CandidateBuilder().WithWordId(2).WithTopicId(1).WithHebrew("שתיים").WithTranslation("два").Build(),
            new CandidateBuilder().WithWordId(3).WithTopicId(1).WithHebrew("שלוש").WithTranslation("три").Build(),
            new CandidateBuilder().WithWordId(4).WithTopicId(1).WithHebrew("ארבע").WithTranslation("четыре").Build()
        };

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 9), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Select(c => c.WordId).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public void Pick_GenderSet_PrefersSameGenderWhenAvailable()
    {
        var correct = new CandidateBuilder()
            .WithWordId(1)
            .WithTopicId(1)
            .WithPartOfSpeech(PartOfSpeech.Noun)
            .WithGender(Gender.Masculine)
            .WithHebrew("אחת")
            .WithTranslation("один")
            .Build();

        var pool = new List<QuizCandidate>();
        for (var i = 2; i <= 4; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(1)
                .WithPartOfSpeech(PartOfSpeech.Noun)
                .WithGender(Gender.Masculine)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        for (var i = 5; i <= 7; i++)
        {
            pool.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithTopicId(1)
                .WithPartOfSpeech(PartOfSpeech.Noun)
                .WithGender(Gender.Feminine)
                .WithHebrew($"מילה{i}")
                .WithTranslation($"слово{i}")
                .Build());
        }

        var picker = new DistractorPicker(new SequenceRandomSource(seed: 10), NullLogger<DistractorPicker>.Instance);

        var result = picker.Pick(correct, pool, QuizMode.HeToRu, 3);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.All(c => c.Gender == Gender.Masculine), Is.True);
    }
}
