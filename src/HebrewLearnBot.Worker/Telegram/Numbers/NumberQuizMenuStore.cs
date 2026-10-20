using System.Collections.Concurrent;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Worker.Telegram.Numbers;

public sealed class NumberQuizMenuStore
{
    private readonly ConcurrentDictionary<long, (NumberQuizConversationData Data, DateTime Expires)> _menus = new();
    private readonly IClock _clock;

    public NumberQuizMenuStore(IClock clock)
    {
        _clock = clock;
    }

    public void Set(long userId, NumberQuizConversationData data)
    {
        foreach (var pair in _menus)
        {
            if (pair.Value.Expires <= _clock.UtcNow)
            {
                _menus.TryRemove(pair.Key, out _);
            }
        }
        if (_menus.Count >= 1000)
        {
            _menus.TryRemove(_menus.First().Key, out _);
        }
        _menus[userId] = (data, _clock.UtcNow.AddMinutes(15));
    }

    public NumberQuizConversationData? Get(long userId)
    {
        return _menus.TryGetValue(userId, out var entry) && entry.Expires > _clock.UtcNow ? entry.Data : null;
    }

    public void Remove(long userId)
    {
        _menus.TryRemove(userId, out _);
    }
}
