using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Text;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class WordImportStore : IWordImportStore
{
    private readonly HebrewDbContext _dbContext;

    public WordImportStore(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WordUpsertOutcome> UpsertAsync(IReadOnlyList<WordImportRow> rows, DateTime nowUtc, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var topicIdByNormalizedName = await EnsureTopicsAsync(rows, nowUtc, cancellationToken);

            var externalKeys = rows
                .Select(row => row.ExternalKey?.Trim())
                .Where(key => !string.IsNullOrEmpty(key))
                .Select(key => key!)
                .Distinct()
                .ToArray();

            var hebrewNormalizedValues = rows
                .Select(row => TextNormalizer.NormalizeHebrew(row.Hebrew))
                .Distinct()
                .ToArray();

            var candidates = await _dbContext.Words
                .Where(word => (word.ExternalKey != null && externalKeys.Contains(word.ExternalKey))
                                || hebrewNormalizedValues.Contains(word.HebrewNormalized))
                .ToListAsync(cancellationToken);

            var byExternalKey = new Dictionary<string, Word>();
            var byHebrewTranslation = new Dictionary<(string Hebrew, string Translation), Word>();

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate.ExternalKey))
                {
                    byExternalKey[candidate.ExternalKey] = candidate;
                }

                byHebrewTranslation[(candidate.HebrewNormalized, candidate.TranslationNormalized)] = candidate;
            }

            var added = 0;
            var updated = 0;

            foreach (var row in rows)
            {
                var hebrewNormalized = TextNormalizer.NormalizeHebrew(row.Hebrew);
                var translationNormalized = TextNormalizer.NormalizeRussian(row.TranslationRu);
                var topicNormalized = TextNormalizer.NormalizeRussian(row.TopicName);
                var topicId = topicIdByNormalizedName[topicNormalized];
                var externalKey = row.ExternalKey?.Trim();

                Word? existing = null;

                if (!string.IsNullOrEmpty(externalKey) && byExternalKey.TryGetValue(externalKey, out var byKey))
                {
                    existing = byKey;
                }
                else if (byHebrewTranslation.TryGetValue((hebrewNormalized, translationNormalized), out var byPair))
                {
                    existing = byPair;
                }

                if (existing is not null)
                {
                    existing.UpdateContent(row.HebrewVocalized, row.Transliteration, row.TranslationRu, row.Gender, nowUtc);
                    existing.MoveToTopic(topicId, nowUtc);
                    existing.ChangePartOfSpeech(row.PartOfSpeech, nowUtc);
                    existing.Activate(nowUtc);

                    updated++;
                }
                else
                {
                    var word = new Word(
                        row.Hebrew,
                        row.HebrewVocalized,
                        row.Transliteration,
                        row.TranslationRu,
                        topicId,
                        row.PartOfSpeech,
                        row.Gender,
                        row.ExternalKey,
                        nowUtc);

                    _dbContext.Words.Add(word);

                    if (!string.IsNullOrEmpty(word.ExternalKey))
                    {
                        byExternalKey[word.ExternalKey] = word;
                    }

                    byHebrewTranslation[(word.HebrewNormalized, word.TranslationNormalized)] = word;

                    added++;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new WordUpsertOutcome(added, updated);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Dictionary<string, int>> EnsureTopicsAsync(IReadOnlyList<WordImportRow> rows, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var uniqueTopicNames = new Dictionary<string, string>();
        foreach (var row in rows)
        {
            var normalized = TextNormalizer.NormalizeRussian(row.TopicName);
            uniqueTopicNames.TryAdd(normalized, row.TopicName);
        }

        var normalizedNames = uniqueTopicNames.Keys.ToArray();

        var existingTopics = await _dbContext.Topics
            .Where(topic => normalizedNames.Contains(topic.NameNormalized))
            .ToListAsync(cancellationToken);

        var topicIdByNormalizedName = existingTopics
            .ToDictionary(topic => topic.NameNormalized, topic => topic.Id);

        var newTopics = new List<Topic>();

        foreach (var (normalized, original) in uniqueTopicNames)
        {
            if (topicIdByNormalizedName.ContainsKey(normalized))
            {
                continue;
            }

            var topic = new Topic(original, nowUtc);
            newTopics.Add(topic);
            _dbContext.Topics.Add(topic);
        }

        if (newTopics.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var topic in newTopics)
            {
                topicIdByNormalizedName[topic.NameNormalized] = topic.Id;
            }
        }

        return topicIdByNormalizedName;
    }
}
