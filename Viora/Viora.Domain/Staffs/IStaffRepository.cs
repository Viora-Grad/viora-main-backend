namespace Viora.Domain.Staffs;

public interface IStaffRepository
{
    void Add(Staff staff);
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    Task<Staff?> GetByUsernameAsync(Guid organizationId, string username, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetOrganizationStaffAsync(Guid organizationId, CancellationToken ct);
    Task<IEnumerable<Staff>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken);
    void Remove(Staff staff);
}
