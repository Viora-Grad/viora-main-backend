namespace Viora.Domain.Services.Internals;

public sealed record ServiceName(string Value)
{
    public static implicit operator ServiceName(string service) => new(service);
    public static implicit operator string(ServiceName service) => service.Value;
}
