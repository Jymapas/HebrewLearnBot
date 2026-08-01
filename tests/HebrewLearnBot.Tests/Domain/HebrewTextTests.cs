using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Tests.Domain;

public sealed class HebrewTextTests
{
    [Test]
    public void HebrewText_StripNiqqud_RemovesVowelPointsAndKeepsMaqaf()
    {
        var result = HebrewText.StripNiqqud("בֵּית־סֵפֶר");

        Assert.That(result, Is.EqualTo("בית־ספר"));
        Assert.That(result, Does.Contain('־'));
    }

    [Test]
    public void HebrewText_IsLatinTransliteration_AcceptsApostropheAndHyphen()
    {
        Assert.That(HebrewText.IsLatinTransliteration("re'shut-ha'ir"), Is.True);
    }

    [Test]
    public void HebrewText_ContainsHebrewLetter_FalseForNiqqudOnly()
    {
        var niqqudOnly = "ְֵָ";

        Assert.That(HebrewText.ContainsHebrewLetter(niqqudOnly), Is.False);
    }
}
