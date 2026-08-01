using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HebrewLearnBot.Infrastructure.Persistence;

public sealed class HebrewDbContextFactory : IDesignTimeDbContextFactory<HebrewDbContext>
{
    public HebrewDbContext CreateDbContext(string[] args)
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var portRaw = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        var port = 5432;
        if (!string.IsNullOrWhiteSpace(portRaw) && int.TryParse(portRaw, out var parsedPort))
        {
            port = parsedPort;
        }

        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "hebrewlearnbot";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "hebrewlearnbot";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        var optionsBuilder = new DbContextOptionsBuilder<HebrewDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(HebrewDbContext).Assembly.FullName));

        return new HebrewDbContext(optionsBuilder.Options);
    }
}
