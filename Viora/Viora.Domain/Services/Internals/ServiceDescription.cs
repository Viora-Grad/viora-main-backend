namespace Viora.Domain.Services.Internals;

public sealed record ServiceDescription(string Value)
{
    public static implicit operator ServiceDescription(string value) => new(value);
    public static implicit operator string(ServiceDescription service) => service.Value;
}
