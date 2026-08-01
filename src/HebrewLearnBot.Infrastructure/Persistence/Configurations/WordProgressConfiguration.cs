using HebrewLearnBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HebrewLearnBot.Infrastructure.Persistence.Configurations;

public sealed class WordProgressConfiguration : IEntityTypeConfiguration<WordProgress>
{
    public void Configure(EntityTypeBuilder<WordProgress> builder)
    {
        builder.ToTable("word_progress");

        builder.Ignore(progress => progress.IsLearned);

        builder.HasKey(progress => new { progress.UserId, progress.WordId });

        builder.Property(progress => progress.UserId)
            .HasColumnName("user_id");

        builder.Property(progress => progress.WordId)
            .HasColumnName("word_id");

        builder.Property(progress => progress.CorrectCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("correct_count");

        builder.Property(progress => progress.WrongCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("wrong_count");

        builder.Property(progress => progress.Streak)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("streak");

        builder.Property(progress => progress.LastShownAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("last_shown_at_utc");

        builder.Property(progress => progress.LastAnswerCorrect)
            .HasColumnName("last_answer_correct");

        builder.Property(progress => progress.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(progress => progress.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasOne<Word>()
            .WithMany()
            .HasForeignKey(progress => progress.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(progress => new { progress.UserId, progress.LastShownAtUtc })
            .HasDatabaseName("ix_word_progress_user_id_last_shown_at_utc");

        builder.HasIndex(progress => progress.WordId)
            .HasDatabaseName("ix_word_progress_word_id");
    }
}
