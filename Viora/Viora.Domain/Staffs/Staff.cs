using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public sealed class Staff : Entity
{
    public Guid BranchId { get; private set; }
    private Staff() { } // For EF Core
    public Staff(Guid id, Guid branchId) : base(id)
    {
        BranchId = branchId;
    }
}
