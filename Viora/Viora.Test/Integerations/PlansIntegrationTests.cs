using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class PlansIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public PlansIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreatePlan_WithFeaturesAndLimitedFeatures_PersistsAllEntities()
    {
        var planId = Guid.NewGuid();
        var plan = Plan.Create(planId, "Pro Plan", "Professional tier", "Full access", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var feature = Feature.Create(Guid.NewGuid(), "appointment_booking", "Book appointments online");
        _dbContext.Add(feature);
        await _dbContext.SaveChangesAsync();

        var planFeature = PlanFeature.Create(Guid.NewGuid(), plan.Id, feature.Id);
        _dbContext.Add(planFeature);
        await _dbContext.SaveChangesAsync();

        var limitedFeatureId = Guid.NewGuid();
        var planLimitedFeature = PlanLimitedFeature.Create(Guid.NewGuid(), plan.Id, limitedFeatureId, 5);
        _dbContext.Add(planLimitedFeature);
        await _dbContext.SaveChangesAsync();

        var trackedPlan = _dbContext.ChangeTracker.Entries<Plan>()
            .FirstOrDefault(e => e.Entity.Id == plan.Id);
        Assert.IsNotNull(trackedPlan);

        var retrievedFeature = await _dbContext.Set<Feature>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == feature.Id);
        Assert.IsNotNull(retrievedFeature);

        var retrievedPlanFeature = await _dbContext.Set<PlanFeature>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pf => pf.Id == planFeature.Id);
        Assert.IsNotNull(retrievedPlanFeature);
        Assert.AreEqual(plan.Id, retrievedPlanFeature.PlanId);
        Assert.AreEqual(feature.Id, retrievedPlanFeature.FeatureId);

        var retrievedPlanLimitedFeature = await _dbContext.Set<PlanLimitedFeature>()
            .AsNoTracking()
            .FirstOrDefaultAsync(plf => plf.Id == planLimitedFeature.Id);
        Assert.IsNotNull(retrievedPlanLimitedFeature);
        Assert.AreEqual(plan.Id, retrievedPlanLimitedFeature.PlanId);
        Assert.AreEqual(limitedFeatureId, retrievedPlanLimitedFeature.LimitedFeatureId);
        Assert.AreEqual(5, retrievedPlanLimitedFeature.LimitValue);
    }

    [TestMethod]
    public async Task CreateFeatureUsage_ForOrganization_PersistsWithCorrectPeriod()
    {
        var limitedFeatureId = Guid.NewGuid();
        var usageResult = FeatureUsage.Create(OrgId, limitedFeatureId, FixedNow, FixedNow.AddDays(30), 10);
        Assert.IsTrue(usageResult.IsSuccess);

        var usage = usageResult.Value;
        _dbContext.Add(usage);
        _dbContext.Entry(usage).Property("RowVersion").CurrentValue = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<FeatureUsage>()
            .FirstOrDefault(e => e.Entity.Id == usage.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(OrgId, tracked.Entity.OrganizationId);
        Assert.AreEqual(limitedFeatureId, tracked.Entity.LimitedFeatureId);
        Assert.AreEqual(10, tracked.Entity.Quota);
        Assert.AreEqual(FixedNow, tracked.Entity.PeriodStart);
        Assert.AreEqual(FixedNow.AddDays(30), tracked.Entity.PeriodEnd);
    }

    [TestMethod]
    public async Task CreateFeatureUsageMany_MultipleLimitedFeatures_PersistsAllRecords()
    {
        var limitedFeatureIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var result = FeatureUsage.CreateMany(OrgId, limitedFeatureIds, FixedNow, FixedNow.AddDays(30), 5);
        Assert.IsTrue(result.IsSuccess);

        var usages = result.Value;
        foreach (var u in usages)
        {
            _dbContext.Add(u);
            _dbContext.Entry(u).Property("RowVersion").CurrentValue = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
        }
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<FeatureUsage>()
            .Where(e => e.Entity.OrganizationId == OrgId)
            .ToList();
        Assert.AreEqual(3, tracked.Count);
    }

    [TestMethod]
    public async Task CreatePlan_WithDifferentPeriods_PersistsPlan()
    {
        var planId = Guid.NewGuid();
        var plan = Plan.Create(planId, "Annual Plan", "Annual tier", "Full access", 199.99m, Currency.Usd, PlanPeriod.annually);
        _dbContext.Add(plan);
        var saved = await _dbContext.SaveChangesAsync();
        Assert.AreEqual(1, saved);

        var trackedPlan = _dbContext.ChangeTracker.Entries<Plan>()
            .FirstOrDefault(e => e.Entity.Id == planId);
        Assert.IsNotNull(trackedPlan);
    }

    [TestMethod]
    public async Task CreatePlan_WithMultipleLimitedFeatures_PersistsCorrectLimits()
    {
        var planId = Guid.NewGuid();
        var plan = Plan.Create(planId, "Enterprise Plan", "Enterprise tier", "Unlimited access", 99.99m, Currency.Usd, PlanPeriod.annually);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var plf1 = PlanLimitedFeature.Create(Guid.NewGuid(), plan.Id, LimitedFeature.Branches.Id, 10);
        var plf2 = PlanLimitedFeature.Create(Guid.NewGuid(), plan.Id, LimitedFeature.StaffMembers.Id, 50);
        var plf3 = PlanLimitedFeature.Create(Guid.NewGuid(), plan.Id, LimitedFeature.StorageBytes.Id, 1073741824);

        _dbContext.AddRange(plf1, plf2, plf3);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<PlanLimitedFeature>()
            .AsNoTracking()
            .Where(plf => plf.PlanId == planId)
            .ToListAsync();
        Assert.AreEqual(3, retrieved.Count);
    }
}
