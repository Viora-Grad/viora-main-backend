namespace Viora.Domain.Forms.Internals;

public record FormName(string value)
{
    public static FormName create(string name)
    {
        return new FormName(name);
    }
}
