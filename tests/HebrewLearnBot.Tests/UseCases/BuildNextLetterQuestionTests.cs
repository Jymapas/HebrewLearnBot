using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Application.UseCases.Alphabet;
using HebrewLearnBot.Tests.Alphabet;
using HebrewLearnBot.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class BuildNextLetterQuestionTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static List<LetterCandidate> BuildCandidates(int count)
    {
        var candidates = new List<LetterCandidate>();
        for (var i = 1; i <= count; i++)
        {
            candidates.Add(new LetterCandidateBuilder()
                .WithLetterId(i)
                .WithGlyph($"glyph{i}")
                .WithName($"Буква{i}")
                .Build());
        }

        return candidates;
    }

    private static (BuildNextLetterQuestion UseCase, FakeLetterCandidateReader Reader) CreateUseCase(
        IReadOnlyList<LetterCandidate> candidates)
    {
        IRandomSource random = new SequenceRandomSource(seed: 42);
        var weightedPicker = new WeightedPicker(random);
        var selector = new LetterNextSelector(weightedPicker);
        var distractorPicker = new LetterDistractorPicker(random);
        var reader = new FakeLetterCandidateReader(candidates);

        var useCase = new BuildNextLetterQuestion(
            reader,
            selector,
            distractorPicker,
            weightedPicker,
            new TestClock(NowUtc),
            NullLogger<BuildNextLetterQuestion>.Instance);

        return (useCase, reader);
    }

    [Test]
    public async Task ExecuteAsync_FewerThanFourCandidates_ReturnsNull()
    {
        var candidates = BuildCandidates(3);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(1, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_EnoughCandidates_ReturnsFourOptions()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(1, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Options, Has.Count.EqualTo(QuizConstants.OptionCount));
    }

    [Test]
    public async Task ExecuteAsync_CorrectIndexPointsAtCorrectLetter()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(1, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Options[result.CorrectIndex].LetterId, Is.EqualTo(result.CorrectLetter.LetterId));
    }

    [Test]
    public async Task ExecuteAsync_OptionLabelsAreLetterNames()
    {
        var candidates = BuildCandidates(8);
        var candidatesById = candidates.ToDictionary(c => c.LetterId);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(1, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        foreach (var option in result!.Options)
        {
            Assert.That(option.Label, Is.EqualTo(candidatesById[option.LetterId].Name));
        }
    }

    [Test]
    public async Task ExecuteAsync_OptionsContainNoDuplicateLetterIds()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(1, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        var distinctIds = result!.Options.Select(option => option.LetterId).Distinct().Count();
        Assert.That(distinctIds, Is.EqualTo(result.Options.Count));
    }

    [Test]
    public async Task ExecuteAsync_PassesUserIdToReader()
    {
        var candidates = BuildCandidates(8);
        var (useCase, reader) = CreateUseCase(candidates);
        var command = new BuildNextLetterQuestionCommand(7, Array.Empty<int>(), 0, 0);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(reader.LastUserId, Is.EqualTo(7));
    }
}
