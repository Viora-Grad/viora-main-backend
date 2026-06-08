using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Viora.Domain.Shared;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure.Seeding.Data;

namespace Viora.Infrastructure.Seeding;

public interface IDatabaseSeeder
{
    public Task SeedAsync(CancellationToken cancellationToken = default);
}
internal class DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        #region Country
        var existingIds = await db.Set<Country>()
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missing = CountriesData.All
            .Where(c => !existingIds.Contains(c.Id))
            .ToList();

        if (missing.Count == 0)
        {
            logger.LogInformation("Country seed: all {Count} countries already present.",
                CountriesData.All.Count);
        }

        db.Set<Country>().AddRange(missing);

        logger.LogInformation("Country seed: inserted {Count} new countries.", missing.Count);
        #endregion Country

        #region Permissions
        var existingPermissions = await db.Set<Permission>()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var missingPermissions = AuthorizationData.Permissions
            .Where(p => !existingPermissions.Contains(p.Id))
            .ToList();

        if (missingPermissions.Count == 0)
        {
            logger.LogInformation("Permission seed: all {Count} permissions already present.",
                AuthorizationData.Permissions.Count);
        }

        db.Set<Permission>().AddRange(missingPermissions);

        logger.LogInformation("Permission seed: inserted {Count} new permissions.", missingPermissions.Count);
        #endregion Permissions

        #region Roles
        var existingRoles = await db.Set<Role>()
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingRoles = AuthorizationData.Roles
            .Where(r => !existingRoles.Contains(r.Id))
            .ToList();

        if (missingRoles.Count == 0)
        {
            logger.LogInformation("Role seed: all {Count} roles already present.",
                AuthorizationData.Roles.Count);
        }

        db.Set<Role>().AddRange(missingRoles);

        logger.LogInformation("Role seed: inserted {Count} new roles.", missingRoles.Count);
        #endregion Roles

        #region RolePermissions

        var existingRolePermissions = await db.Set<RolePermission>()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(cancellationToken);

        var missingRolePermissions = AuthorizationData.RolePermissions
            .Where(rp => !existingRolePermissions.Any(existing =>
                existing.RoleId == rp.RoleId && existing.PermissionId == rp.PermissionId))
            .ToList();

        var missingCount = missingRolePermissions.Count;
        if (missingCount == 0)
        {
            logger.LogInformation("RolePermission seed: all {Count} role-permissions already present.",
                AuthorizationData.RolePermissions.Count);
            return;

        }
        db.Set<RolePermission>().AddRange(missingRolePermissions);
        #endregion RolePermissions

        await db.SaveChangesAsync(cancellationToken);

    }
}
