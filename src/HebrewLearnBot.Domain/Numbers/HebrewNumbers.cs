namespace HebrewLearnBot.Domain.Numbers;

public static class HebrewNumbers
{
    public static IReadOnlyList<HebrewNumberInfo> All { get; } = Build();

    public static HebrewNumberInfo Get(int value)
    {
        if (value is < 0 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        return All[value];
    }

    private static IReadOnlyList<HebrewNumberInfo> Build()
    {
        string[] units = ["אפס", "אחת", "שתיים", "שלוש", "ארבע", "חמש", "שש", "שבע", "שמונה", "תשע", "עשר"];
        string[] unitTransliterations = ["efes", "akhat", "shtayim", "shalosh", "arba", "khamesh", "shesh", "sheva", "shmone", "tesha", "eser"];
        string[] teens = ["אחת עשרה", "שתים עשרה", "שלוש עשרה", "ארבע עשרה", "חמש עשרה", "שש עשרה", "שבע עשרה", "שמונה עשרה", "תשע עשרה"];
        string[] teenTransliterations = ["akhat esre", "shtem esre", "shlosh esre", "arba esre", "khamesh esre", "shesh esre", "shva esre", "shmone esre", "tsha esre"];
        string[] tens = ["", "", "עשרים", "שלושים", "ארבעים", "חמישים", "שישים", "שבעים", "שמונים", "תשעים"];
        string[] tenTransliterations = ["", "", "esrim", "shloshim", "arbaim", "khamishim", "shishim", "shivim", "shmonim", "tishim"];
        var result = new List<HebrewNumberInfo>(100);

        for (var value = 0; value <= 99; value++)
        {
            if (value <= 10)
            {
                result.Add(new HebrewNumberInfo(value, units[value], unitTransliterations[value]));
            }
            else if (value < 20)
            {
                result.Add(new HebrewNumberInfo(value, teens[value - 11], teenTransliterations[value - 11]));
            }
            else
            {
                var ten = value / 10;
                var unit = value % 10;
                var hebrew = unit == 0 ? tens[ten] : $"{tens[ten]} ו{units[unit]}";
                var transliteration = unit == 0
                    ? tenTransliterations[ten]
                    : $"{tenTransliterations[ten]} ve-{unitTransliterations[unit]}";
                result.Add(new HebrewNumberInfo(value, hebrew, transliteration));
            }
        }

        return result.AsReadOnly();
    }
}
