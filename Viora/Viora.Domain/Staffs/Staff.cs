using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public class Staff : Entity
{
    public Guid BranchId { get; private set; }

    private Staff(Guid id, Guid branchId) : base(id)
    {
        BranchId = branchId;
    }
}
