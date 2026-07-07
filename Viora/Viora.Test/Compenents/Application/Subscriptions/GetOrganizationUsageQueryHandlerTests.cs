using System.Reflection;
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Subscriptions.GetOrganizationUsage;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Features.Internal;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the GetOrganizationUsageQueryHandler covering success, no-org, and no-subscription scenarios.
/// </summary>
[TestClass]
public sealed class GetOrganizationUsageQueryHandlerTests
{
    private readonly Mock<IFeatureUsageRepository> _featureUsageRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly GetOrganizationUsageQueryHandler _handler;

    public GetOrganizationUsageQueryHandlerTests()
    {
        _handler = new GetOrganizationUsageQueryHandler(
            _featureUsageRepoMock.Object,
            _subscriptionRepoMock.Object,
            _planRepoMock.Object,
            _userContextMock.Object);
    }

    // ===== Handle_NoOrganizationId_ReturnsFailure =====

    /// <summary>
    /// Verifies that Handle returns a failure result when userContext.OrganizationId is null.
    /// </summary>
    [TestMethod]
    public async Task Handle_NoOrganizationId_ReturnsFailure()
    {
        // Arrange
        _userContextMock.Setup(c => c.OrganizationId).Returns((Guid?)null);

        // Act
        Result<OrganizationUsageResponse> result = await _handler.Handle(
            new GetOrganizationUsageQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("Subscription.OrganizationMissing", result.Error.Name);
    }

    // ===== Handle_WithFeatures_ReturnsUsageResponse =====

    /// <summary>
    /// Verifies that Handle returns a correct usage response with features, limits, and used values when a subscription and plan exist.
    /// </summary>
    [TestMethod]
    public async Task Handle_WithFeatures_ReturnsUsageResponse()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        DateTime periodEnd = now.AddMonths(1);

        _userContextMock.Setup(c => c.OrganizationId).Returns(orgId);

        var branchesFeature = LimitedFeature.Branches;
        var staffFeature = LimitedFeature.StaffMembers;

        var usage1 = FeatureUsage.Create(orgId, branchesFeature.Id, now, periodEnd, 5).Value;
        var usage2 = FeatureUsage.Create(orgId, staffFeature.Id, now, periodEnd, 20).Value;

        _featureUsageRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureUsage> { usage1, usage2 });

        var subscription = Subscription.Create(planId, orgId, now, periodEnd).Value;
        _subscriptionRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var planLimitedFeature1 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, branchesFeature.Id, 10);
        var planLimitedFeature2 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, staffFeature.Id, 30);

        var plan = Plan.Create(planId, "Test Plan", "A plan", "content", 99.99m, Currency.Usd, PlanPeriod.monthly);

        PropertyInfo planLimitedFeaturesProp = typeof(Plan).GetProperty(nameof(Plan.PlanLimitedFeatures))!;
        planLimitedFeaturesProp.SetValue(plan, new List<PlanLimitedFeature> { planLimitedFeature1, planLimitedFeature2 });

        _planRepoMock
            .Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<OrganizationUsageResponse> result = await _handler.Handle(
            new GetOrganizationUsageQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(orgId, result.Value.OrganizationId);
        Assert.AreEqual(2, result.Value.Features.Count);

        FeatureUsageResponse branchesResponse = result.Value.Features.First(f => f.Key == "branches");
        Assert.AreEqual(branchesFeature.Id, branchesResponse.LimitedFeatureId);
        Assert.AreEqual("branches", branchesResponse.Key);
        Assert.AreEqual(branchesFeature.Description.value, branchesResponse.Description);
        Assert.AreEqual(5, branchesResponse.Remaining);
        Assert.AreEqual(10, branchesResponse.Limit);
        Assert.AreEqual(5, branchesResponse.Used);
        Assert.AreEqual(now, branchesResponse.PeriodStart);
        Assert.AreEqual(periodEnd, branchesResponse.PeriodEnd);

        FeatureUsageResponse staffResponse = result.Value.Features.First(f => f.Key == "staff_members");
        Assert.AreEqual(staffFeature.Id, staffResponse.LimitedFeatureId);
        Assert.AreEqual("staff_members", staffResponse.Key);
        Assert.AreEqual(staffFeature.Description.value, staffResponse.Description);
        Assert.AreEqual(20, staffResponse.Remaining);
        Assert.AreEqual(30, staffResponse.Limit);
        Assert.AreEqual(10, staffResponse.Used);
    }

    // ===== Handle_NoSubscription_ReturnsEmptyLimits =====

    /// <summary>
    /// Verifies that Handle returns features with null Limit and null Used when no subscription exists.
    /// </summary>
    [TestMethod]
    public async Task Handle_NoSubscription_ReturnsEmptyLimits()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        DateTime periodEnd = now.AddMonths(1);

        _userContextMock.Setup(c => c.OrganizationId).Returns(orgId);

        var branchesFeature = LimitedFeature.Branches;
        var usage = FeatureUsage.Create(orgId, branchesFeature.Id, now, periodEnd, 5).Value;

        _featureUsageRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureUsage> { usage });

        _subscriptionRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        Result<OrganizationUsageResponse> result = await _handler.Handle(
            new GetOrganizationUsageQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Features.Count);

        FeatureUsageResponse feature = result.Value.Features[0];
        Assert.AreEqual(branchesFeature.Id, feature.LimitedFeatureId);
        Assert.AreEqual("branches", feature.Key);
        Assert.AreEqual(branchesFeature.Description.value, feature.Description);
        Assert.AreEqual(5, feature.Remaining);
        Assert.IsNull(feature.Limit);
        Assert.IsNull(feature.Used);
        Assert.AreEqual(now, feature.PeriodStart);
        Assert.AreEqual(periodEnd, feature.PeriodEnd);
    }
}
