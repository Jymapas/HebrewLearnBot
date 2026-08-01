using HebrewLearnBot.Worker.Config;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class AdminGuard
{
    private readonly long _mainAdminId;

    public AdminGuard(AppOptions options)
    {
        _mainAdminId = options.MainAdminId;
    }

    public bool IsAdmin(long userId)
    {
        return userId == _mainAdminId;
    }
}
