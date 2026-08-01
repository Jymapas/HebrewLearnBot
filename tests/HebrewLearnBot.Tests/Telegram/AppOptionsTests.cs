using HebrewLearnBot.Worker.Config;
using Microsoft.Extensions.Configuration;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class AppOptionsTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Test]
    public void FromConfiguration_MissingBotToken_ThrowsInvalidOperationException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["MAIN_ADMIN_ID"] = "123",
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_DB"] = "hebrew",
            ["POSTGRES_USER"] = "hebrew_user"
        });

        Assert.That(() => AppOptions.FromConfiguration(configuration), Throws.InvalidOperationException);
    }

    [Test]
    public void FromConfiguration_NonNumericMainAdminId_ThrowsInvalidOperationException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BOT_TOKEN"] = "token",
            ["MAIN_ADMIN_ID"] = "not-a-number",
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_DB"] = "hebrew",
            ["POSTGRES_USER"] = "hebrew_user"
        });

        Assert.That(() => AppOptions.FromConfiguration(configuration), Throws.InvalidOperationException);
    }

    [Test]
    public void FromConfiguration_ValidValues_BuildsConnectionString()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BOT_TOKEN"] = "token",
            ["MAIN_ADMIN_ID"] = "123",
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_PORT"] = "5433",
            ["POSTGRES_DB"] = "hebrew",
            ["POSTGRES_USER"] = "hebrew_user",
            ["POSTGRES_PASSWORD"] = "secret"
        });

        var options = AppOptions.FromConfiguration(configuration);

        Assert.That(
            options.PostgresConnectionString,
            Is.EqualTo("Host=localhost;Port=5433;Database=hebrew;Username=hebrew_user;Password=secret"));
        Assert.That(options.MainAdminId, Is.EqualTo(123));
    }

    [Test]
    public void FromConfiguration_NoPassword_OmitsPasswordFromConnectionString()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BOT_TOKEN"] = "token",
            ["MAIN_ADMIN_ID"] = "123",
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_DB"] = "hebrew",
            ["POSTGRES_USER"] = "hebrew_user"
        });

        var options = AppOptions.FromConfiguration(configuration);

        Assert.That(
            options.PostgresConnectionString,
            Is.EqualTo("Host=localhost;Port=5432;Database=hebrew;Username=hebrew_user"));
    }

    [Test]
    public void FromConfiguration_DefaultsApplied_WhenOptionalVarsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BOT_TOKEN"] = "token",
            ["MAIN_ADMIN_ID"] = "123",
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_DB"] = "hebrew",
            ["POSTGRES_USER"] = "hebrew_user"
        });

        var options = AppOptions.FromConfiguration(configuration);

        Assert.That(options.TimeZone, Is.EqualTo("Europe/Moscow"));
        Assert.That(options.QuizCandidatePoolLimit, Is.EqualTo(2000));
        Assert.That(options.ImportMaxFileBytes, Is.EqualTo(1048576));
        Assert.That(options.ImportMaxRows, Is.EqualTo(5000));
    }
}
