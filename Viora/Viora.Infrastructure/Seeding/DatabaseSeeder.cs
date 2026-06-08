using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Viora.Domain.Plans.Features;
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
        var existingCountryIds = await db.Set<Country>()
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missingCountries = CountriesData.All
            .Where(c => !existingCountryIds.Contains(c.Id))
            .ToList();

        if (missingCountries.Count == 0)
            logger.LogInformation("Country seed: all {Count} countries already present.", CountriesData.All.Count);
        else
        {
            await db.Set<Country>().AddRangeAsync(missingCountries, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Country seed: inserted {Count} new countries.", missingCountries.Count);
        }
        #endregion Country

        #region LimitedFeatures
        var existingFeatureIds = await db.Set<LimitedFeature>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingFeatures = LimitedFeaturesData.All
            .Where(f => !existingFeatureIds.Contains(f.Id))
            .ToList();

        if (missingFeatures.Count == 0)
            logger.LogInformation("LimitedFeature seed: all {Count} features already present.", LimitedFeaturesData.All.Count);
        else
        {
            await db.Set<LimitedFeature>().AddRangeAsync(missingFeatures, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("LimitedFeature seed: inserted {Count} new features.", missingFeatures.Count);
        }
        #endregion LimitedFeatures

        #region Roles
        var existingRoleIds = await db.Set<Role>()
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingRoles = AuthorizationData.Roles
            .Where(r => !existingRoleIds.Contains(r.Id))
            .Select(r => new Role(r.Id, r.Name))
            .ToList();

        if (missingRoles.Count == 0)
            logger.LogInformation("Role seed: all {Count} roles already present.", AuthorizationData.Roles.Count);
        else
        {
            await db.Set<Role>().AddRangeAsync(missingRoles, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Role seed: inserted {Count} new roles.", missingRoles.Count);
        }
        #endregion Roles

        #region Permissions
        var existingPermissionIds = await db.Set<Permission>()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var missingPermissions = AuthorizationData.Permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .Select(p => Permission.Create(p.Id, p.Name))
            .ToList();

        if (missingPermissions.Count == 0)
            logger.LogInformation("Permission seed: all {Count} permissions already present.", AuthorizationData.Permissions.Count);
        else
        {
            await db.Set<Permission>().AddRangeAsync(missingPermissions, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Permission seed: inserted {Count} new permissions.", missingPermissions.Count);
        }
        #endregion Permissions

        #region RolePermissions
        var existingRolePermissions = await db.Set<RolePermission>()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(cancellationToken);

        var missingRolePermissions = AuthorizationData.RolePermissions
            .Where(rp => !existingRolePermissions.Any(e => e.RoleId == rp.RoleId && e.PermissionId == rp.PermissionId))
            .ToList();

        if (missingRolePermissions.Count == 0)
            logger.LogInformation("RolePermission seed: all {Count} role-permissions already present.", AuthorizationData.RolePermissions.Count);
        else
        {
            await db.Set<RolePermission>().AddRangeAsync(missingRolePermissions, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("RolePermission seed: inserted {Count} new role-permissions.", missingRolePermissions.Count);
        }
        #endregion RolePermissions
    }
}
