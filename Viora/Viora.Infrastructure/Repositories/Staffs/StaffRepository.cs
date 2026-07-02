using Microsoft.EntityFrameworkCore;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Infrastructure.Repositories.Staffs;

internal class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Staff>> GetBranchServiceStaffsAsync(Guid branchId, Guid serviceId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>()
            .Where(s => s.Branches.Any(b => b.Id == branchId) && s.Services.Any(sv => sv.Id == serviceId) && s.StaffStatus == StaffStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>().Where(s => s.Branches.Any(b => b.Id == branchId)).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>().Where(s => s.Roles.Any(r => r.Id == roleId)).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Staff?> GetByUsernameAsync(Guid organizationId, string username, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>().FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.Username == username, cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetOrganizationStaffAsync(Guid organizationId, CancellationToken ct)
    {
        return await DbContext.Set<Staff>().Where(s => s.OrganizationId == organizationId).AsNoTracking().ToListAsync(ct);
    }

}
