using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Abstractions.Telegram;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Import;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Application.UseCases.Alphabet;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Application.UseCases.Statistics;
using HebrewLearnBot.Application.UseCases.Topics;
using HebrewLearnBot.Application.UseCases.Words;
using HebrewLearnBot.Infrastructure.Persistence;
using HebrewLearnBot.Infrastructure.Persistence.Readers;
using HebrewLearnBot.Infrastructure.Persistence.Repositories;
using HebrewLearnBot.Infrastructure.Randomness;
using HebrewLearnBot.Infrastructure.Telegram;
using HebrewLearnBot.Infrastructure.Time;
using HebrewLearnBot.Worker.Config;
using HebrewLearnBot.Worker.Hosting;
using HebrewLearnBot.Worker.Logging;
using HebrewLearnBot.Worker.Telegram;
using HebrewLearnBot.Worker.Telegram.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;

SecretsEnvLoader.Load();

Log.Logger = SerilogSetup.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.WebHost.UseUrls("http://*:8080");

    var services = builder.Services;
    var options = AppOptions.FromConfiguration(builder.Configuration);
    services.AddSingleton(options);

    services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(options.BotToken));
    services.AddSingleton<AdminGuard>();
    services.AddSingleton<UpdateDispatcher>();
    services.AddScoped<CommandHandlers>();
    services.AddScoped<MessageHandlers>();
    services.AddScoped<CallbackHandlers>();
    services.AddScoped<DocumentHandlers>();
    services.AddHostedService<BotHostedService>();

    services.AddDbContext<HebrewDbContext>(db =>
        db.UseNpgsql(options.PostgresConnectionString,
            npgsql => npgsql.MigrationsAssembly("HebrewLearnBot.Infrastructure")));

    // Инфраструктурные — Singleton
    services.AddSingleton<IClock, SystemClock>();
    services.AddSingleton<IRandomSource, SystemRandomSource>();
    services.AddSingleton(new QuizPoolOptions { CandidatePoolLimit = options.QuizCandidatePoolLimit });
    services.AddSingleton<WeightedPicker>();
    services.AddSingleton<NextWordSelector>();
    services.AddSingleton<DistractorPicker>();
    services.AddSingleton<WordCsvParser>();
    services.AddSingleton<LetterNextSelector>();
    services.AddSingleton<LetterDistractorPicker>();

    // Репозитории и ридеры — Scoped
    services.AddScoped<ITopicRepository, TopicRepository>();
    services.AddScoped<IWordRepository, WordRepository>();
    services.AddScoped<IWordProgressRepository, WordProgressRepository>();
    services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
    services.AddScoped<IConversationStateRepository, ConversationStateRepository>();
    services.AddScoped<IWordImportStore, WordImportStore>();
    services.AddScoped<IQuizCandidateReader, QuizCandidateReader>();
    services.AddScoped<ITelegramFileDownloader, TelegramFileDownloader>();
    services.AddScoped<ILetterCandidateReader, LetterCandidateReader>();
    services.AddScoped<ILetterProgressRepository, LetterProgressRepository>();

    // UseCase'ы — Scoped
    services.AddScoped<BuildNextQuestion>();
    services.AddScoped<BuildQuizSummary>();
    services.AddScoped<SkipQuizQuestion>();
    services.AddScoped<SubmitQuizAnswer>();
    services.AddScoped<GetUserSettings>();
    services.AddScoped<SetQuizMode>();
    services.AddScoped<SetTopic>();
    services.AddScoped<ListTopics>();
    services.AddScoped<CountWords>();
    services.AddScoped<ImportWords>();
    services.AddScoped<GetUserStatistics>();
    services.AddScoped<BuildNextLetterQuestion>();
    services.AddScoped<SubmitLetterAnswer>();
    services.AddScoped<SkipLetterQuestion>();
    services.AddScoped<BuildLetterQuizSummary>();
    services.AddScoped<GetLetterStatistics>();

    services.AddScoped<QuizPresenter>();
    services.AddScoped<AlphabetQuizPresenter>();

    var app = builder.Build();

    app.MapGet("/health", () => Results.Ok("ok"));

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HebrewDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
