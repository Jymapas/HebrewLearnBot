using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.Quiz;

public sealed class WeightedPickerTests
{
    [Test]
    public void PickIndex_SingleCandidate_ReturnsZero()
    {
        var picker = new WeightedPicker(new SequenceRandomSource(0.5));

        var index = picker.PickIndex(new[] { 3.0 });

        Assert.That(index, Is.EqualTo(0));
    }

    [Test]
    public void PickIndex_EqualWeights_DistributesRoughlyUniformly()
    {
        var picker = new WeightedPicker(new SequenceRandomSource(seed: 42));
        var weights = new[] { 1.0, 1.0, 1.0, 1.0 };
        var counts = new int[4];

        for (var i = 0; i < 10000; i++)
        {
            counts[picker.PickIndex(weights)]++;
        }

        foreach (var count in counts)
        {
            Assert.That(count, Is.InRange(2100, 2900));
        }
    }

    [Test]
    public void PickIndex_HeavilySkewedWeights_PicksHeavyItemMoreOften()
    {
        var picker = new WeightedPicker(new SequenceRandomSource(seed: 7));
        var weights = new[] { 50.0, 0.05, 0.05, 0.05 };
        var heavyWins = 0;

        for (var i = 0; i < 2000; i++)
        {
            if (picker.PickIndex(weights) == 0)
            {
                heavyWins++;
            }
        }

        Assert.That(heavyWins, Is.GreaterThanOrEqualTo(1800));
    }

    [Test]
    public void PickIndex_EmptyWeights_ThrowsArgumentException()
    {
        var picker = new WeightedPicker(new SequenceRandomSource(0.5));

        Assert.That(() => picker.PickIndex(Array.Empty<double>()), Throws.ArgumentException);
    }

    [Test]
    public void PickIndex_AllWeightsZero_StillReturnsValidIndex()
    {
        var picker = new WeightedPicker(new SequenceRandomSource(0.3, 0.7, 0.1));

        var index = picker.PickIndex(new[] { 0.0, 0.0, 0.0 });

        Assert.That(index, Is.InRange(0, 2));
    }

    [Test]
    public void Shuffle_WithSeededSource_IsDeterministic()
    {
        var itemsA = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var itemsB = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var pickerA = new WeightedPicker(new SequenceRandomSource(seed: 123));
        var pickerB = new WeightedPicker(new SequenceRandomSource(seed: 123));

        pickerA.Shuffle(itemsA);
        pickerB.Shuffle(itemsB);

        Assert.That(itemsA, Is.EqualTo(itemsB));
    }
}
