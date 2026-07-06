using Microsoft.EntityFrameworkCore;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Infrastructure.Repositories.Staffs;

internal class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // Eager-loads the full object graph for the "me" profile. Split query avoids the cartesian blow-up
    // across the three independent collections; owned branch collections (phones, hours, services-provided)
    // load automatically with the branch.
    public async Task<Staff?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.Roles).ThenInclude(r => r.Permissions)
            .Include(s => s.Branches)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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
    public override async Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Staff>()
            .Include(s => s.Roles).ThenInclude(r => r.Permissions)
            .Include(s => s.Branches)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

}
