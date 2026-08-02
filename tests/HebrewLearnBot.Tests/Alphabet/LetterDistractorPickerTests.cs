using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.Alphabet;

public sealed class LetterDistractorPickerTests
{
    [Test]
    public void Pick_EnoughLetters_ReturnsRequestedCount()
    {
        var correct = new LetterCandidateBuilder().WithLetterId(1).Build();

        var pool = new List<LetterCandidate> { correct };
        for (var i = 2; i <= 10; i++)
        {
            pool.Add(new LetterCandidateBuilder().WithLetterId(i).WithGlyph($"letter{i}").WithName($"Name{i}").Build());
        }

        var picker = new LetterDistractorPicker(new SequenceRandomSource(seed: 1));

        var result = picker.Pick(correct, pool, 3);

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void Pick_NeverIncludesCorrectLetter()
    {
        var correct = new LetterCandidateBuilder().WithLetterId(1).Build();

        var pool = new List<LetterCandidate> { correct };
        for (var i = 2; i <= 6; i++)
        {
            pool.Add(new LetterCandidateBuilder().WithLetterId(i).WithGlyph($"letter{i}").WithName($"Name{i}").Build());
        }

        var picker = new LetterDistractorPicker(new SequenceRandomSource(seed: 6));

        var result = picker.Pick(correct, pool, 3);

        Assert.That(result.Any(c => c.LetterId == correct.LetterId), Is.False);
    }

    [Test]
    public void Pick_FewerLettersThanRequested_ReturnsAllAvailable()
    {
        var correct = new LetterCandidateBuilder().WithLetterId(1).Build();

        var pool = new List<LetterCandidate>
        {
            correct,
            new LetterCandidateBuilder().WithLetterId(2).WithGlyph("letter2").WithName("Name2").Build(),
            new LetterCandidateBuilder().WithLetterId(3).WithGlyph("letter3").WithName("Name3").Build()
        };

        var picker = new LetterDistractorPicker(new SequenceRandomSource(seed: 8));

        var result = picker.Pick(correct, pool, 3);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void Pick_ZeroDesiredCount_ReturnsEmpty()
    {
        var correct = new LetterCandidateBuilder().WithLetterId(1).Build();
        var pool = new List<LetterCandidate> { correct };

        var picker = new LetterDistractorPicker(new SequenceRandomSource(seed: 1));

        var result = picker.Pick(correct, pool, 0);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Pick_ResultLetterIdsAreDistinct()
    {
        var correct = new LetterCandidateBuilder().WithLetterId(1).Build();

        var pool = new List<LetterCandidate> { correct };
        for (var i = 2; i <= 8; i++)
        {
            pool.Add(new LetterCandidateBuilder().WithLetterId(i).WithGlyph($"letter{i}").WithName($"Name{i}").Build());
        }

        var picker = new LetterDistractorPicker(new SequenceRandomSource(seed: 3));

        var result = picker.Pick(correct, pool, 3);

        Assert.That(result.Select(c => c.LetterId).Distinct().Count(), Is.EqualTo(result.Count));
    }
}
