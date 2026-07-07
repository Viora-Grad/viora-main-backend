using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.GetOrganizationSubscriptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the GetOrganizationSubscriptionsQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetOrganizationSubscriptionsQueryHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly GetOrganizationSubscriptionsQueryHandler _handler;

    public GetOrganizationSubscriptionsQueryHandlerTests()
    {
        _handler = new GetOrganizationSubscriptionsQueryHandler(
            _subscriptionRepoMock.Object,
            _organizationRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid organization ID returns subscription responses.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidOrganizationId_ReturnsSubscriptionResponses()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);
        var subscriptions = new List<Subscription>
        {
            CreateTestSubscription(orgId)
        };

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        Result<List<SubscriptionResponse>> result = await _handler.Handle(
            new GetOrganizationSubscriptionsQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Count);
    }

    /// <summary>
    /// Verifies that Handle returns correct subscription properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidOrganizationId_ReturnsCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);
        var subscription = CreateTestSubscription(orgId, planId);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { subscription });

        // Act
        Result<List<SubscriptionResponse>> result = await _handler.Handle(
            new GetOrganizationSubscriptionsQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        SubscriptionResponse response = result.Value[0];
        Assert.AreEqual(subscription.Id, response.Id);
        Assert.AreEqual(planId, response.PlanId);
        Assert.AreEqual(orgId, response.OrganizationId);
        Assert.AreEqual("Active", response.Status);
    }

    /// <summary>
    /// Verifies that Handle with non-existent organization throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetOrganizationSubscriptionsQuery(orgId), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with organization having no subscriptions throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationHasNoSubscriptions_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Subscription>?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetOrganizationSubscriptionsQuery(orgId), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to repositories.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoriesWithCancellationToken()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);
        var cts = new CancellationTokenSource();

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, cts.Token))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, cts.Token))
            .ReturnsAsync(new List<Subscription>());

        // Act
        await _handler.Handle(new GetOrganizationSubscriptionsQuery(orgId), cts.Token);

        // Assert
        _organizationRepoMock.Verify(r => r.GetByIdAsync(orgId, cts.Token), Times.Once);
        _subscriptionRepoMock.Verify(r => r.GetAllByOrganizationIdAsync(orgId, cts.Token), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle returns multiple subscriptions for an organization.
    /// </summary>
    [TestMethod]
    public async Task Handle_MultipleSubscriptions_ReturnsAll()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);
        var subscriptions = new List<Subscription>
        {
            CreateTestSubscription(orgId),
            CreateTestSubscription(orgId)
        };

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        Result<List<SubscriptionResponse>> result = await _handler.Handle(
            new GetOrganizationSubscriptionsQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

    private static Subscription CreateTestSubscription(Guid orgId, Guid? planId = null)
    {
        return Subscription.Create(
            planId ?? Guid.NewGuid(),
            orgId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1)).Value;
    }
}
