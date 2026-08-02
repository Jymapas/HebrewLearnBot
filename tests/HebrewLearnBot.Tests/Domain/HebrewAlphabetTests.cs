using HebrewLearnBot.Domain.Alphabet;

namespace HebrewLearnBot.Tests.Domain;

public sealed class HebrewAlphabetTests
{
    [Test]
    public void All_ContainsThirtyOneLetters()
    {
        Assert.That(HebrewAlphabet.All, Has.Count.EqualTo(31));
    }

    [Test]
    public void All_IdsAreUniqueAndSequential()
    {
        var ids = HebrewAlphabet.All.Select(letter => letter.Id).OrderBy(id => id).ToList();

        Assert.That(ids, Is.EqualTo(Enumerable.Range(1, 31)));
    }

    [Test]
    public void All_GlyphsAreUnique()
    {
        var glyphs = HebrewAlphabet.All.Select(letter => letter.Glyph).Distinct().Count();

        Assert.That(glyphs, Is.EqualTo(HebrewAlphabet.All.Count));
    }

    [Test]
    public void All_NamesAreUnique()
    {
        var names = HebrewAlphabet.All.Select(letter => letter.Name).Distinct().Count();

        Assert.That(names, Is.EqualTo(HebrewAlphabet.All.Count));
    }

    [Test]
    public void All_ExactlyFiveFinalForms()
    {
        Assert.That(HebrewAlphabet.All.Count(letter => letter.IsFinal), Is.EqualTo(5));
    }

    [Test]
    public void All_NoGlyphOrNameIsEmpty()
    {
        Assert.That(HebrewAlphabet.All.All(letter => !string.IsNullOrWhiteSpace(letter.Glyph)), Is.True);
        Assert.That(HebrewAlphabet.All.All(letter => !string.IsNullOrWhiteSpace(letter.Name)), Is.True);
    }
}
