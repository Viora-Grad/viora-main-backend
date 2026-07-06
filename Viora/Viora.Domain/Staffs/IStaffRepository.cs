using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public interface IStaffRepository
{
    void Add(Staff staff);
    Task<Staff?> GetByIdAsync(Guid? id, CancellationToken cancellationToken);

    /// <summary>Loads a staff member with roles (and their permissions), branches, and services eager-loaded.</summary>
    Task<Staff?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetBranchServiceStaffsAsync(Guid branchId, Guid serviceId, CancellationToken cancellationToken);
    Task<Staff?> GetByUsernameAsync(Guid organizationId, string username, CancellationToken cancellationToken);
    Task<IEnumerable<Staff>> GetOrganizationStaffAsync(Guid organizationId, CancellationToken ct);
    Task<IEnumerable<Staff>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Staff>> ListAsync(ISpecification<Staff> spec, CancellationToken cancellationToken = default);
    void Remove(Staff staff);
}
