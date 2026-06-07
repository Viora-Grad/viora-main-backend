namespace Viora.Domain.Organizations.OnBoardings.Internals;

public record ServiceDescription(string Value)
{
    public static implicit operator ServiceDescription(string value) => new(value);
    public static implicit operator string(ServiceDescription serviceDescription) => serviceDescription.Value;
}
