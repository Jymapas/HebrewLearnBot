using HebrewLearnBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HebrewLearnBot.Infrastructure.Persistence.Configurations;

public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");

        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id)
            .HasColumnName("id");

        builder.Property(topic => topic.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(topic => topic.NameNormalized)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name_normalized");

        builder.Property(topic => topic.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(topic => topic.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasIndex(topic => topic.NameNormalized)
            .IsUnique()
            .HasDatabaseName("ux_topics_name_normalized");
    }
}
