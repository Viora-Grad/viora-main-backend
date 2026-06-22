namespace Viora.Domain.Plans.Features.Internal;

public record FeatureKey(string value)
{

    public static FeatureKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Feature key cannot be null or empty.", nameof(value));
        return new FeatureKey(value);
    }
}

