namespace Viora.Domain.Subscriptions.Addons.Internal;

public record AddonType(int Id, string Value)
{
    public static readonly AddonType TimeBase = new AddonType(1, "TimeBase");
    public static readonly AddonType OneTime = new AddonType(2, "OneTime");


    public static AddonType FromId(int id)
    {
        return id switch
        {
            1 => TimeBase,
            2 => OneTime,
            _ => throw new ArgumentException($"Invalid AddonType Id: {id}")
        };
    }

}

