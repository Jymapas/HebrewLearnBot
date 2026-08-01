using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace HebrewLearnBot.Worker.Config;

public sealed class AppOptions
{
    public string BotToken { get; init; } = string.Empty;
    public long MainAdminId { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string PostgresConnectionString { get; init; } = string.Empty;
    public int QuizCandidatePoolLimit { get; init; }
    public long ImportMaxFileBytes { get; init; }
    public int ImportMaxRows { get; init; }

    public static AppOptions FromConfiguration(IConfiguration configuration)
    {
        var botToken = configuration["BOT_TOKEN"]?.Trim();
        var mainAdminIdRaw = configuration["MAIN_ADMIN_ID"]?.Trim();
        var timeZone = configuration["TZ"]?.Trim();
        var dbHost = configuration["POSTGRES_HOST"]?.Trim();
        var dbPortRaw = configuration["POSTGRES_PORT"]?.Trim();
        var dbName = configuration["POSTGRES_DB"]?.Trim();
        var dbUser = configuration["POSTGRES_USER"]?.Trim();
        var dbPassword = configuration["POSTGRES_PASSWORD"]?.Trim();
        var quizPoolLimitRaw = configuration["QUIZ_CANDIDATE_POOL_LIMIT"]?.Trim();
        var importMaxFileBytesRaw = configuration["IMPORT_MAX_FILE_BYTES"]?.Trim();
        var importMaxRowsRaw = configuration["IMPORT_MAX_ROWS"]?.Trim();

        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException("BOT_TOKEN is required.");
        }

        if (string.IsNullOrWhiteSpace(mainAdminIdRaw))
        {
            throw new InvalidOperationException("MAIN_ADMIN_ID is required.");
        }

        if (!long.TryParse(mainAdminIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mainAdminId))
        {
            throw new InvalidOperationException("MAIN_ADMIN_ID must be a valid numeric id.");
        }

        if (string.IsNullOrWhiteSpace(dbHost))
        {
            throw new InvalidOperationException("POSTGRES_HOST is required.");
        }

        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException("POSTGRES_DB is required.");
        }

        if (string.IsNullOrWhiteSpace(dbUser))
        {
            throw new InvalidOperationException("POSTGRES_USER is required.");
        }

        var port = 5432;
        if (!string.IsNullOrWhiteSpace(dbPortRaw)
            && !int.TryParse(dbPortRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
        {
            throw new InvalidOperationException("POSTGRES_PORT must be a valid number.");
        }

        var connectionString = string.IsNullOrWhiteSpace(dbPassword)
            ? $"Host={dbHost};Port={port};Database={dbName};Username={dbUser}"
            : $"Host={dbHost};Port={port};Database={dbName};Username={dbUser};Password={dbPassword}";

        var quizCandidatePoolLimit = 2000;
        if (!string.IsNullOrWhiteSpace(quizPoolLimitRaw))
        {
            if (!int.TryParse(quizPoolLimitRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out quizCandidatePoolLimit)
                || quizCandidatePoolLimit <= 0)
            {
                throw new InvalidOperationException("QUIZ_CANDIDATE_POOL_LIMIT must be a positive integer when provided.");
            }
        }

        long importMaxFileBytes = 1048576;
        if (!string.IsNullOrWhiteSpace(importMaxFileBytesRaw))
        {
            if (!long.TryParse(importMaxFileBytesRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out importMaxFileBytes)
                || importMaxFileBytes <= 0)
            {
                throw new InvalidOperationException("IMPORT_MAX_FILE_BYTES must be a positive integer when provided.");
            }
        }

        var importMaxRows = 5000;
        if (!string.IsNullOrWhiteSpace(importMaxRowsRaw))
        {
            if (!int.TryParse(importMaxRowsRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out importMaxRows)
                || importMaxRows <= 0)
            {
                throw new InvalidOperationException("IMPORT_MAX_ROWS must be a positive integer when provided.");
            }
        }

        return new AppOptions
        {
            BotToken = botToken,
            MainAdminId = mainAdminId,
            TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "Europe/Moscow" : timeZone,
            PostgresConnectionString = connectionString,
            QuizCandidatePoolLimit = quizCandidatePoolLimit,
            ImportMaxFileBytes = importMaxFileBytes,
            ImportMaxRows = importMaxRows
        };
    }
}
