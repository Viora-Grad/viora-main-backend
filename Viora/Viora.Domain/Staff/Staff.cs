using Viora.Domain.Abstractions;

namespace Viora.Domain.Staff;

public class Staff : Entity
{
    public Guid BranchId { get; private set; }
    public Staff() { }// EF Core
    public Staff(Guid id, Guid branchId) : base(id)
    {
        this.BranchId = branchId;
    }
}
