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
        new(1, "א", "Алеф", false),
        new(2, "ב" + Dagesh, "Бет", false),
        new(3, "ב", "Вет", false),
        new(4, "ג", "Гимель", false),
        new(5, "ד", "Далет", false),
        new(6, "ה", "Хей", false),
        new(7, "ו", "Вав", false),
        new(8, "ז", "Зайин", false),
        new(9, "ח", "Хет", false),
        new(10, "ט", "Тет", false),
        new(11, "י", "Йод", false),
        new(12, "כ" + Dagesh, "Каф", false),
        new(13, "כ", "Хаф", false),
        new(14, "ל", "Ламед", false),
        new(15, "מ", "Мем", false),
        new(16, "נ", "Нун", false),
        new(17, "ס", "Самех", false),
        new(18, "ע", "Айин", false),
        new(19, "פ" + Dagesh, "Пей", false),
        new(20, "פ", "Фей", false),
        new(21, "צ", "Цади", false),
        new(22, "ק", "Куф", false),
        new(23, "ר", "Реш", false),
        new(24, "ש" + ShinDot, "Шин", false),
        new(25, "ש" + SinDot, "Син", false),
        new(26, "ת", "Тав", false),
        new(27, "ך", "Каф-софит", true),
        new(28, "ם", "Мем-софит", true),
        new(29, "ן", "Нун-софит", true),
        new(30, "ף", "Пей-софит", true),
        new(31, "ץ", "Цади-софит", true),
    ];
}
