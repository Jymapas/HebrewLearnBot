using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HebrewLearnBot.Infrastructure.Persistence.Configurations;

public sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(settings => settings.UserId);
        builder.Property(settings => settings.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");

        builder.Property(settings => settings.SelectedMode)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("selected_mode");

        builder.Property(settings => settings.SelectedTopicId)
            .HasColumnName("selected_topic_id");

        builder.Property(settings => settings.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(settings => settings.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(settings => settings.SelectedTopicId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
