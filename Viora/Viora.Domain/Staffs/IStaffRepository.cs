using Viora.Domain.Users.Identity;

namespace Viora.Domain.Staffs;

public interface IStaffRepository
{
    void Add(Staff staff);
    void Add(Role role);
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    void Remove(Staff staff);
    void Remove(Role role);
}
