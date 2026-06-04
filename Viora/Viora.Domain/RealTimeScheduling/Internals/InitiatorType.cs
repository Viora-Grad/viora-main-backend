namespace Viora.Domain.RealTimeScheduling.Internals;

public record InitiatorType(string Value)
{
    public static readonly InitiatorType System = new("System");
    public static readonly InitiatorType Client = new("Client");
}
