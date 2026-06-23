using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public sealed class Staff : Entity
{
    private Staff() { } // For EF Core
    public Staff(Guid id) : base(id) { }
}
