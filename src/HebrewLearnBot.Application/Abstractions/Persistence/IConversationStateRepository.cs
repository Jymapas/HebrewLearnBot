using HebrewLearnBot.Domain.Entities;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IConversationStateRepository
{
    Task<ConversationState?> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task UpsertAsync(long userId, string conversationName, string step, string? stateJson, DateTime nowUtc, CancellationToken cancellationToken);

    Task DeleteAsync(long userId, CancellationToken cancellationToken);
}
