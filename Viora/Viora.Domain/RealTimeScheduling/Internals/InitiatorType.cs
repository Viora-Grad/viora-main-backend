namespace Viora.Domain.RealTimeScheduling.Internals;

public record InitiatorType(string Value)
{
    public static readonly InitiatorType System = new("System");
    public static readonly InitiatorType Client = new("Client");

    public static InitiatorType FromValue(string value)
    {
        return value switch
        {
            "System" => System,
            "Client" => Client,
            _ => throw new InvalidOperationException(
                $"Unknown InitiatorType '{value}'.")
        };
    }


}
