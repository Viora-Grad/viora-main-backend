namespace Viora.Domain.MedicalRecords.Internal;

public sealed record Allergy
{
    public string Value { get; init; }
    public Allergy(string value)
    {
        Value = value.ToLower();
    }
}
