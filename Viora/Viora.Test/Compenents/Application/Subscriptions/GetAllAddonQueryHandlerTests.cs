using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.GetAddons;
using Viora.Application.Subscriptions.GetFeatureAddon;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the GetAllAddonQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetAllAddonQueryHandlerTests
{
    private readonly Mock<ILimitedFeatureAddonRepository> _addonRepoMock = new();
    private readonly GetAllAddonQueryHandler _handler;

    public GetAllAddonQueryHandlerTests()
    {
        _handler = new GetAllAddonQueryHandler(_addonRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with existing addons returns a list of FeatureAddonResponse objects.
    /// </summary>
    [TestMethod]
    public async Task Handle_AddonsExist_ReturnsFeatureAddonResponses()
    {
        // Arrange
        var addons = new List<LimitedFeatureAddon>
        {
            LimitedFeatureAddon.Create(Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 10, new Money(5.00m, Currency.Usd)),
            LimitedFeatureAddon.Create(Guid.NewGuid(), Guid.NewGuid(), AddonType.TimeBase, 20, new Money(10.00m, Currency.Usd))
        };

        _addonRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(addons);

        // Act
        Result<List<FeatureAddonResponse>> result = await _handler.Handle(new GetAllAddonsQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }

    /// <summary>
    /// Verifies that Handle returns correct addon properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_AddonsExist_ReturnsCorrectProperties()
    {
        // Arrange
        Guid addonId = Guid.NewGuid();
        Guid limitedFeatureId = Guid.NewGuid();
        var addon = LimitedFeatureAddon.Create(addonId, limitedFeatureId, AddonType.OneTime, 15, new Money(7.50m, Currency.Usd));

        _addonRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });

        // Act
        Result<List<FeatureAddonResponse>> result = await _handler.Handle(new GetAllAddonsQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        FeatureAddonResponse response = result.Value[0];
        Assert.AreEqual(addonId, response.id);
        Assert.AreEqual(limitedFeatureId, response.LimitedFeatureId);
        Assert.AreEqual(15, response.AdditionalQuota);
        Assert.AreEqual(7.50m, response.Price.amount);
        Assert.AreEqual("USD", response.Price.currency);
    }

    /// <summary>
    /// Verifies that Handle with no addons throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_NoAddons_ThrowsNotFoundException()
    {
        // Arrange
        _addonRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetAllAddonsQuery(), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the repository.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoryWithCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _addonRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(cts.Token))
            .ReturnsAsync(new List<LimitedFeatureAddon>
            {
                LimitedFeatureAddon.Create(Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 5, new Money(1m, Currency.Usd))
            });

        // Act
        await _handler.Handle(new GetAllAddonsQuery(), cts.Token);

        // Assert
        _addonRepoMock.Verify(r => r.GetAllAsNoTrackingAsync(cts.Token), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle returns addons with different types mapped correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_MultipleAddonTypes_ReturnsAllMapped()
    {
        // Arrange
        var addons = new List<LimitedFeatureAddon>
        {
            LimitedFeatureAddon.Create(Guid.NewGuid(), Guid.NewGuid(), AddonType.TimeBase, 10, new Money(5m, Currency.Usd)),
            LimitedFeatureAddon.Create(Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 20, new Money(10m, Currency.Usd))
        };

        _addonRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(addons);

        // Act
        Result<List<FeatureAddonResponse>> result = await _handler.Handle(new GetAllAddonsQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }
}
