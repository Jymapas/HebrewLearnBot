using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HebrewLearnBot.Infrastructure.Persistence.Configurations;

public sealed class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("words");

        builder.HasKey(word => word.Id);
        builder.Property(word => word.Id)
            .HasColumnName("id");

        builder.Property(word => word.Hebrew)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("hebrew");

        builder.Property(word => word.HebrewNormalized)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("hebrew_normalized");

        builder.Property(word => word.HebrewVocalized)
            .HasMaxLength(200)
            .HasColumnName("hebrew_vocalized");

        builder.Property(word => word.Transliteration)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("transliteration");

        builder.Property(word => word.TranslationRu)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("translation_ru");

        builder.Property(word => word.TranslationNormalized)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("translation_normalized");

        builder.Property(word => word.TopicId)
            .IsRequired()
            .HasColumnName("topic_id");

        builder.Property(word => word.PartOfSpeech)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("part_of_speech");

        builder.Property(word => word.Gender)
            .HasConversion<int?>()
            .HasColumnName("gender");

        builder.Property(word => word.ExternalKey)
            .HasMaxLength(64)
            .HasColumnName("external_key");

        // Без HasDefaultValue: CLR-дефолт bool равен false, поэтому дефолт БД перекрыл бы
        // деактивацию ещё не сохранённого слова. Значение всегда задаёт домен.
        builder.Property(word => word.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(word => word.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(word => word.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(word => word.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(word => new { word.HebrewNormalized, word.TranslationNormalized })
            .IsUnique()
            .HasDatabaseName("ux_words_hebrew_translation");

        builder.HasIndex(word => word.ExternalKey)
            .IsUnique()
            .HasDatabaseName("ux_words_external_key")
            .HasFilter("external_key IS NOT NULL");

        builder.HasIndex(word => new { word.TopicId, word.IsActive })
            .HasDatabaseName("ix_words_topic_id_is_active");
    }
}
