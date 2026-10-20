using HebrewLearnBot.Domain.Numbers;

namespace HebrewLearnBot.Tests.Domain;

public sealed class HebrewNumbersTests
{
    [Test]
    public void All_Catalog_ContainsEveryValueExactlyOnce()
    {
        Assert.That(HebrewNumbers.All.Select(item => item.Value), Is.EqualTo(Enumerable.Range(0, 100)));
        Assert.That(HebrewNumbers.All.Select(item => item.Hebrew).Distinct().Count(), Is.EqualTo(100));
        Assert.That(HebrewNumbers.All.All(item => !string.IsNullOrWhiteSpace(item.Transliteration)), Is.True);
    }

    [TestCase(0, "אפס", "efes")]
    [TestCase(2, "שתיים", "shtayim")]
    [TestCase(10, "עשר", "eser")]
    [TestCase(11, "אחת עשרה", "akhat esre")]
    [TestCase(12, "שתים עשרה", "shtem esre")]
    [TestCase(13, "שלוש עשרה", "shlosh esre")]
    [TestCase(17, "שבע עשרה", "shva esre")]
    [TestCase(19, "תשע עשרה", "tsha esre")]
    [TestCase(20, "עשרים", "esrim")]
    [TestCase(22, "עשרים ושתיים", "esrim ve-shtayim")]
    [TestCase(38, "שלושים ושמונה", "shloshim ve-shmone")]
    [TestCase(99, "תשעים ותשע", "tishim ve-tesha")]
    public void Get_GrammarBoundary_ReturnsFeminineForm(int value, string hebrew, string transliteration)
    {
        Assert.That(HebrewNumbers.Get(value), Is.EqualTo(new HebrewNumberInfo(value, hebrew, transliteration)));
    }

    [TestCase(-1)]
    [TestCase(100)]
    public void Get_OutOfRange_Throws(int value)
    {
        Assert.That(() => HebrewNumbers.Get(value), Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
