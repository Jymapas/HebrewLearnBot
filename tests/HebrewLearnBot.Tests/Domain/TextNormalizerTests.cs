using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Tests.Domain;

public sealed class TextNormalizerTests
{
    [Test]
    public void TextNormalizer_NormalizeRussian_TrimsLowercasesCollapsesSpacesAndReplacesYo()
    {
        var result = TextNormalizer.NormalizeRussian("  Ёлка   зелёная  ");

        Assert.That(result, Is.EqualTo("елка зеленая"));
    }

    [Test]
    public void TextNormalizer_NormalizeHebrew_StripsNiqqudAndCollapsesSpaces()
    {
        var result = TextNormalizer.NormalizeHebrew("  שׁוּלְחָן   גדול  ");

        Assert.That(result, Is.EqualTo("שולחן גדול"));
    }
}
