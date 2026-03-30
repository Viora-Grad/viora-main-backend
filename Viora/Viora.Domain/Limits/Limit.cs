using Viora.Domain.Abstractions;
using Viora.Domain.Limits.Internal;

namespace Viora.Domain.Limits;

public class Limit : Entity
{
    public LimitKey Key { get; private set; }
    public LimitDescription Description { get; private set; }

    private Limit(Guid Id, LimitKey key, LimitDescription description) : base(Id)
    {
        this.Key = key;
        this.Description = description;
    }
}
