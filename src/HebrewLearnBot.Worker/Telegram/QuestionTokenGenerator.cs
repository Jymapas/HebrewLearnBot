using System.Security.Cryptography;

namespace HebrewLearnBot.Worker.Telegram;

public static class QuestionTokenGenerator
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
    }
}
