namespace HebrewLearnBot.Domain.Alphabet;

public static class HebrewAlphabet
{
    // Дагеш и точки шин/син различают буквы, которые без огласовки выглядят одинаково
    // (ב/בּ, כ/כּ, פ/פּ, ש с точкой справа/слева) — здесь их не убираем, в отличие от
    // HebrewText.StripNiqqud для слов: без этого знака вопрос неоднозначен.
    private const char Dagesh = 'ּ';
    private const char ShinDot = 'ׁ';
    private const char SinDot = 'ׂ';

    // Id стабильны и не должны переиспользоваться/меняться — на них ссылается letter_progress
    // без внешнего ключа (буквы не таблица в БД).
    public static readonly IReadOnlyList<HebrewLetterInfo> All =
    [
        new HebrewLetterInfo(1, "א", "Алеф", false),
        new HebrewLetterInfo(2, "ב" + Dagesh, "Бет", false),
        new HebrewLetterInfo(3, "ב", "Вет", false),
        new HebrewLetterInfo(4, "ג", "Гимель", false),
        new HebrewLetterInfo(5, "ד", "Далет", false),
        new HebrewLetterInfo(6, "ה", "Хей", false),
        new HebrewLetterInfo(7, "ו", "Вав", false),
        new HebrewLetterInfo(8, "ז", "Зайин", false),
        new HebrewLetterInfo(9, "ח", "Хет", false),
        new HebrewLetterInfo(10, "ט", "Тет", false),
        new HebrewLetterInfo(11, "י", "Йод", false),
        new HebrewLetterInfo(12, "כ" + Dagesh, "Каф", false),
        new HebrewLetterInfo(13, "כ", "Хаф", false),
        new HebrewLetterInfo(14, "ל", "Ламед", false),
        new HebrewLetterInfo(15, "מ", "Мем", false),
        new HebrewLetterInfo(16, "נ", "Нун", false),
        new HebrewLetterInfo(17, "ס", "Самех", false),
        new HebrewLetterInfo(18, "ע", "Айин", false),
        new HebrewLetterInfo(19, "פ" + Dagesh, "Пе", false),
        new HebrewLetterInfo(20, "פ", "Фе", false),
        new HebrewLetterInfo(21, "צ", "Цади", false),
        new HebrewLetterInfo(22, "ק", "Куф", false),
        new HebrewLetterInfo(23, "ר", "Реш", false),
        new HebrewLetterInfo(24, "ש" + ShinDot, "Шин", false),
        new HebrewLetterInfo(25, "ש" + SinDot, "Син", false),
        new HebrewLetterInfo(26, "ת", "Тав", false),
        new HebrewLetterInfo(27, "ך", "Каф-софит", true),
        new HebrewLetterInfo(28, "ם", "Мем-софит", true),
        new HebrewLetterInfo(29, "ן", "Нун-софит", true),
        new HebrewLetterInfo(30, "ף", "Пе-софит", true),
        new HebrewLetterInfo(31, "ץ", "Цади-софит", true),
    ];
}
