namespace Viora.Domain.Plans.Features.Internal;

public record FeatureDescription(string value)
{
    public static FeatureDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Feature description cannot be null or empty.", nameof(value));
        return new FeatureDescription(value);
    }
}

