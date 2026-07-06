using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Internal;
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
        var existingLimitedFeatureIds = await db.Set<LimitedFeature>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingLimitedFeatures = LimitedFeaturesData.All
            .Where(f => !existingLimitedFeatureIds.Contains(f.Id))
            .ToList();

        if (missingLimitedFeatures.Count == 0)
            logger.LogInformation("LimitedFeature seed: all {Count} features already present.", LimitedFeaturesData.All.Count);
        else
        {
            await db.Set<LimitedFeature>().AddRangeAsync(missingLimitedFeatures, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("LimitedFeature seed: inserted {Count} new features.", missingLimitedFeatures.Count);
        }
        #endregion LimitedFeatures

        #region Roles
        var existingRoleIds = await db.Set<Role>()
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingRoles = Role.All
            .Where(r => !existingRoleIds.Contains(r.Id))
            .ToList();

        if (missingRoles.Count == 0)
        {
            logger.LogInformation("Role seed: roles already present.");
        }
        else
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Role ON", cancellationToken);
                foreach (var role in missingRoles)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        "INSERT INTO Role (Id, Name, Description, TenantId) VALUES (@id, @name, @description, @tenantId)",
                        new SqlParameter("@id", role.Id),
                        new SqlParameter("@name", role.Name),
                        new SqlParameter("@description", (object?)role.Description ?? DBNull.Value),
                        new SqlParameter("@tenantId", (object?)role.TenantId ?? DBNull.Value));
                }
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Role OFF", cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(ex, "Role seed: error occurred while inserting roles.");
                throw;

            }
        }
        #endregion Roles

        #region Permissions
        var permissions = await db.Set<Permission>().ToListAsync(cancellationToken);
        var existingPermissionIds = permissions.Select(p => p.Id).ToList();

        var missingPermissions = AuthorizationData.Permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .Select(p => Permission.Create(p.Id, p.Name, p.Description))
            .ToList();

        var permissionsWithoutDescription = permissions.Where(p => string.IsNullOrEmpty(p.Description)).ToList();

        permissionsWithoutDescription.ForEach(p =>
        {
            var updatedPermission = AuthorizationData.Permissions.FirstOrDefault(ap => ap.Id == p.Id);
            if (updatedPermission != null && !string.IsNullOrEmpty(updatedPermission.Description))
            {
                p.Description = updatedPermission.Description;
            }
        });

        if (missingPermissions.Count == 0)
            logger.LogInformation("Permission seed: all {Count} permissions already present.", AuthorizationData.Permissions.Count);
        else
        {
            await db.Set<Permission>().AddRangeAsync(missingPermissions, cancellationToken);
            logger.LogInformation("Permission seed: inserted {Count} new permissions.", missingPermissions.Count);
        }
        await db.SaveChangesAsync(cancellationToken);
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

        #region plan
        var existingPlanIds = await db.Set<Plan>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingPlans = PlanData.All
            .Where(f => !existingPlanIds.Contains(f.Id))
            .ToList();

        if (missingPlans.Count == 0)
            logger.LogInformation("Plan seed: all {Count} plans already present.", PlanData.All.Count);
        else
        {
            await db.Set<Plan>().AddRangeAsync(missingPlans, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Plan seed: inserted {Count} new plans.", missingPlans.Count);
        }
        #endregion

        #region Feature
        var existingFeatureIds = await db.Set<Feature>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingFeatures = FeatureData.All
            .Where(f => !existingFeatureIds.Contains(f.Id))
            .ToList();

        if (missingFeatures.Count == 0)
            logger.LogInformation("Feature seed: all {Count} features already present.", FeatureData.All.Count);
        else
        {
            await db.Set<Feature>().AddRangeAsync(missingFeatures, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Feature seed: inserted {Count} new features.", missingFeatures.Count);
        }

        #endregion Feature

        #region PlanFeature
        var existingPlanFeatureIds = await db.Set<PlanFeature>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingPlanFeatures = PlanFeatureData.All
            .Where(f => !existingPlanFeatureIds.Contains(f.Id))
            .ToList();

        if (missingPlanFeatures.Count == 0)
            logger.LogInformation("PlanFeature seed: all {Count} plan-features already present.", PlanFeatureData.All.Count);
        else
        {
            await db.Set<PlanFeature>().AddRangeAsync(missingPlanFeatures, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("PlanFeature seed: inserted {Count} new plan-features.", missingPlanFeatures.Count);
        }
        #endregion PlanFeature

        #region PlanLimitedFeature
        var existingPlanLimitedFeatureIds = await db.Set<PlanLimitedFeature>()
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var missingPlanLimitedFeatures = PlanLimitedFeatureData.All
            .Where(f => !existingPlanLimitedFeatureIds.Contains(f.Id))
            .ToList();

        if (missingPlanLimitedFeatures.Count == 0)
            logger.LogInformation("PlanLimitedFeature seed: all {Count} plan-limited-features already present.", PlanLimitedFeatureData.All.Count);
        else
        {
            await db.Set<PlanLimitedFeature>().AddRangeAsync(missingPlanLimitedFeatures, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("PlanLimitedFeature seed: inserted {Count} new plan-limited-features.", missingPlanLimitedFeatures.Count);
        }

        #endregion PlanLimitedFeature

        #region MarketingQuotaBackfill
        // FeatureUsage rows are normally created at subscription time. Orgs that subscribed before the
        // marketing-posts feature existed have no row and would hit FeatureUsageNotFound -> 405 on finalize.
        // Idempotently provision one for each active subscription whose plan grants the feature.
        var marketingFeatureId = LimitedFeature.MarketingAiPosts.Id;

        var marketingGrants = await db.Set<PlanLimitedFeature>()
            .Where(plf => plf.LimitedFeatureId == marketingFeatureId)
            .ToDictionaryAsync(plf => plf.PlanId, plf => plf.LimitValue, cancellationToken);

        if (marketingGrants.Count == 0)
        {
            logger.LogInformation("Marketing quota backfill: no plan grants the marketing-posts feature; nothing to backfill.");
        }
        else
        {
            var orgsWithMarketingUsage = (await db.Set<FeatureUsage>()
                .Where(fu => fu.LimitedFeatureId == marketingFeatureId)
                .Select(fu => fu.OrganizationId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            var activeSubscriptions = await db.Set<Subscription>()
                .Where(s => s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            var newUsages = new List<FeatureUsage>();
            foreach (var subscription in activeSubscriptions)
            {
                if (orgsWithMarketingUsage.Contains(subscription.OrganizationId))
                    continue;
                if (!marketingGrants.TryGetValue(subscription.PlanId, out var limit))
                    continue;

                var usage = FeatureUsage.Create(
                    subscription.OrganizationId,
                    marketingFeatureId,
                    subscription.SubscriptionsStartTime,
                    subscription.SubscriptionsEndTime,
                    limit);

                if (usage.IsFailure)
                {
                    logger.LogWarning("Marketing quota backfill: could not create usage for org {Org}: {Error}.",
                        subscription.OrganizationId, usage.Error.Name);
                    continue;
                }

                newUsages.Add(usage.Value);
                orgsWithMarketingUsage.Add(subscription.OrganizationId); // guard against duplicate active subs
            }

            if (newUsages.Count == 0)
                logger.LogInformation("Marketing quota backfill: all eligible organizations already provisioned.");
            else
            {
                await db.Set<FeatureUsage>().AddRangeAsync(newUsages, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Marketing quota backfill: provisioned {Count} organizations.", newUsages.Count);
            }
        }
        #endregion MarketingQuotaBackfill

        #region Addons
        var existingAddon = await db.Set<LimitedFeatureAddon>()
      .Select(f => f.Id)
      .ToListAsync(cancellationToken);

        var missingAddons = AddonData.All
            .Where(f => !existingAddon.Contains(f.Id))
            .ToList();

        if (missingAddons.Count == 0)
            logger.LogInformation("Addon seed: all {Count} addons already present.", AddonData.All.Count);
        else
        {
            await db.Set<LimitedFeatureAddon>().AddRangeAsync(missingAddons, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Addon seed: inserted {Count} new addons.", missingAddons.Count);
        }
        #endregion Addons

        static object NullOrValue(object? value) => value ?? DBNull.Value;
    }

}
