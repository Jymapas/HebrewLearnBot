using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Tests.Fakes;
using HebrewLearnBot.Tests.Quiz;
using Microsoft.Extensions.Logging.Abstractions;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class BuildNextQuestionTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static List<QuizCandidate> BuildCandidates(int count)
    {
        var candidates = new List<QuizCandidate>();
        for (var i = 1; i <= count; i++)
        {
            candidates.Add(new CandidateBuilder()
                .WithWordId(i)
                .WithHebrew($"מילה{i}", $"מילה{i}")
                .WithTransliteration($"word{i}")
                .WithTranslation($"перевод{i}", $"перевод{i}")
                .Build());
        }

        return candidates;
    }

    private static (BuildNextQuestion UseCase, FakeQuizCandidateReader Reader) CreateUseCase(
        IReadOnlyList<QuizCandidate> candidates,
        QuizPoolOptions? poolOptions = null)
    {
        IRandomSource random = new SequenceRandomSource(seed: 42);
        var weightedPicker = new WeightedPicker(random);
        var selector = new NextWordSelector(weightedPicker);
        var distractorPicker = new DistractorPicker(random, NullLogger<DistractorPicker>.Instance);
        var reader = new FakeQuizCandidateReader(candidates);

        var useCase = new BuildNextQuestion(
            reader,
            selector,
            distractorPicker,
            weightedPicker,
            new TestClock(NowUtc),
            poolOptions ?? new QuizPoolOptions(),
            NullLogger<BuildNextQuestion>.Instance);

        return (useCase, reader);
    }

    [Test]
    public async Task ExecuteAsync_FewerThanFourCandidates_ReturnsNull()
    {
        var candidates = BuildCandidates(3);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_EnoughCandidates_ReturnsFourOptions()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Options, Has.Count.EqualTo(QuizConstants.OptionCount));
    }

    [Test]
    public async Task ExecuteAsync_CorrectIndexPointsAtCorrectWord()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Options[result.CorrectIndex].WordId, Is.EqualTo(result.CorrectWord.WordId));
    }

    [Test]
    public async Task ExecuteAsync_TopicFilterSet_PassesTopicIdToReader()
    {
        var candidates = BuildCandidates(8);
        var (useCase, reader) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, 7, Array.Empty<int>(), 0, 0);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(reader.LastTopicId, Is.EqualTo(7));
    }

    [Test]
    public async Task ExecuteAsync_RuToHeMode_OptionLabelsHaveNoTransliteration()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.RuToHe, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        foreach (var option in result!.Options)
        {
            Assert.That(option.Label, Does.Not.Contain("("));
        }
    }

    [Test]
    public async Task ExecuteAsync_HeToRuMode_OptionLabelsAreRussianTranslations()
    {
        var candidates = BuildCandidates(8);
        var candidatesById = candidates.ToDictionary(c => c.WordId);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        foreach (var option in result!.Options)
        {
            Assert.That(option.Label, Is.EqualTo(candidatesById[option.WordId].TranslationRu));
        }
    }

    [Test]
    public async Task ExecuteAsync_OptionsContainNoDuplicateWordIds()
    {
        var candidates = BuildCandidates(8);
        var (useCase, _) = CreateUseCase(candidates);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        var distinctIds = result!.Options.Select(option => option.WordId).Distinct().Count();
        Assert.That(distinctIds, Is.EqualTo(result.Options.Count));
    }

    [Test]
    public async Task ExecuteAsync_UsesConfiguredPoolLimit()
    {
        var candidates = BuildCandidates(8);
        var poolOptions = new QuizPoolOptions { CandidatePoolLimit = 42 };
        var (useCase, reader) = CreateUseCase(candidates, poolOptions);
        var command = new BuildNextQuestionCommand(1, QuizMode.HeToRu, null, Array.Empty<int>(), 0, 0);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(reader.LastLimit, Is.EqualTo(42));
    }
}
