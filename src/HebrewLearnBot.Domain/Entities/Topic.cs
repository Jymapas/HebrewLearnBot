using HebrewLearnBot.Domain.Exceptions;
using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Domain.Entities;

public sealed class Topic
{
    private const int MaxNameLength = 100;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NameNormalized { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Topic()
    {
    }

    public Topic(string name, DateTime createdAtUtc)
    {
        ValidateName(name);

        Name = name.Trim();
        NameNormalized = TextNormalizer.NormalizeRussian(name);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public void Rename(string name, DateTime updatedAtUtc)
    {
        ValidateName(name);

        Name = name.Trim();
        NameNormalized = TextNormalizer.NormalizeRussian(name);
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Topic name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new DomainException("Topic name must be 100 characters or shorter.");
        }
    }
}
