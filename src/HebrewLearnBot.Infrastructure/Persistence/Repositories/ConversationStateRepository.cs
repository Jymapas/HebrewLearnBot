using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class ConversationStateRepository : IConversationStateRepository
{
    private readonly HebrewDbContext _dbContext;

    public ConversationStateRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConversationState?> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ConversationStates
            .AsNoTracking()
            .FirstOrDefaultAsync(state => state.UserId == userId, cancellationToken);
    }

    public async Task UpsertAsync(long userId, string conversationName, string step, string? stateJson, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ConversationStates
            .FirstOrDefaultAsync(state => state.UserId == userId, cancellationToken);

        if (existing is null)
        {
            _dbContext.ConversationStates.Add(new ConversationState(userId, conversationName, step, stateJson, nowUtc));
        }
        else if (existing.ConversationName == conversationName)
        {
            existing.Update(step, stateJson, nowUtc);
        }
        else
        {
            _dbContext.ConversationStates.Remove(existing);
            _dbContext.ConversationStates.Add(new ConversationState(userId, conversationName, step, stateJson, nowUtc));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long userId, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ConversationStates
            .FirstOrDefaultAsync(state => state.UserId == userId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        _dbContext.ConversationStates.Remove(existing);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
