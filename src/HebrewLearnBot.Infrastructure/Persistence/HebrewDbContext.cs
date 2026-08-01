using HebrewLearnBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence;

public sealed class HebrewDbContext : DbContext
{
    public HebrewDbContext(DbContextOptions<HebrewDbContext> options)
        : base(options)
    {
    }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<WordProgress> WordProgress => Set<WordProgress>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<ConversationState> ConversationStates => Set<ConversationState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HebrewDbContext).Assembly);
    }
}
