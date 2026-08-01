using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Tests.Domain;

public sealed class WordTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Word_HebrewWithoutHebrewLetters_ThrowsDomainException()
    {
        Assert.That(
            () => new Word("стол", null, "shulchan", "стол", 1, PartOfSpeech.Noun, Gender.Masculine, null, CreatedAtUtc),
            Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Word_HebrewWithNiqqud_StoresStrippedHebrewAndKeepsVocalized()
    {
        var word = new Word("שׁוּלְחָן", "שׁוּלְחָן", "shulchan", "стол", 1, PartOfSpeech.Noun, Gender.Masculine, null, CreatedAtUtc);

        Assert.That(word.Hebrew, Is.EqualTo("שולחן"));
        Assert.That(word.HebrewVocalized, Is.EqualTo("שׁוּלְחָן"));
    }

    [Test]
    public void Word_TransliterationWithCyrillic_ThrowsDomainException()
    {
        Assert.That(
            () => new Word("שולחן", null, "шулхан", "стол", 1, PartOfSpeech.Noun, Gender.Masculine, null, CreatedAtUtc),
            Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Word_GenderOnNonNoun_ThrowsDomainException()
    {
        Assert.That(
            () => new Word("לרוץ", null, "larutz", "бежать", 1, PartOfSpeech.Verb, Gender.Feminine, null, CreatedAtUtc),
            Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Word_TranslationWithYo_NormalizesToYe()
    {
        var word = new Word("אילן", null, "ilan", "Ёлка", 1, PartOfSpeech.Noun, Gender.Feminine, null, CreatedAtUtc);

        Assert.That(word.TranslationNormalized, Is.EqualTo("елка"));
    }

    [Test]
    public void Word_ChangePartOfSpeechToVerb_ResetsGender()
    {
        var word = new Word("שולחן", null, "shulchan", "стол", 1, PartOfSpeech.Noun, Gender.Masculine, null, CreatedAtUtc);

        word.ChangePartOfSpeech(PartOfSpeech.Verb, UpdatedAtUtc);

        Assert.That(word.PartOfSpeech, Is.EqualTo(PartOfSpeech.Verb));
        Assert.That(word.Gender, Is.Null);
    }

    [Test]
    public void Word_EmptyExternalKey_StoredAsNull()
    {
        var word = new Word("שולחן", null, "shulchan", "стол", 1, PartOfSpeech.Noun, Gender.Masculine, "   ", CreatedAtUtc);

        Assert.That(word.ExternalKey, Is.Null);
    }
}
