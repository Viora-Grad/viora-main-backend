using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Plans.GetLimitedFeature;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features;

namespace Viora.Test.Compenents.Application.Plans;

/// <summary>
/// Unit tests for the GetLimitedFeatureByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetLimitedFeatureByIdQueryHandlerTests
{
    private readonly Mock<ILimitedFeatureRepository> _limitedFeatureRepoMock = new();
    private readonly GetLimitedFeatureByIdQueryHandler _handler;

    public GetLimitedFeatureByIdQueryHandlerTests()
    {
        _handler = new GetLimitedFeatureByIdQueryHandler(_limitedFeatureRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with a valid limited feature ID returns a FeatureResponse with correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidId_ReturnsFeatureResponseWithCorrectProperties()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.StaffMembers;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        // Act
        Result<FeatureResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(limitedFeature.Id, result.Value.Id);
        Assert.AreEqual("staff_members", result.Value.Key);
        Assert.AreEqual("Number of staff members the organization can have", result.Value.Description);
    }

    /// <summary>
    /// Verifies that Handle with the Branches limited feature returns correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_BranchesLimitedFeature_ReturnsCorrectProperties()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.Branches;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        // Act
        Result<FeatureResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("branches", result.Value.Key);
        Assert.AreEqual("Number of branches the organization can have", result.Value.Description);
    }

    /// <summary>
    /// Verifies that Handle with the StorageBytes limited feature returns correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_StorageBytesLimitedFeature_ReturnsCorrectProperties()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.StorageBytes;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        // Act
        Result<FeatureResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("storage_gb", result.Value.Key);
        Assert.AreEqual("Storage quota in Bytes", result.Value.Description);
    }

    /// <summary>
    /// Verifies that Handle with a non-existent limited feature ID throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_LimitedFeatureNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var query = new GetLimitedFeatureByIdQuery(id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LimitedFeature?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the repository.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoryWithCancellationToken()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.MarketingAiPosts;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);
        var cts = new CancellationTokenSource();

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, cts.Token))
            .ReturnsAsync(limitedFeature);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _limitedFeatureRepoMock.Verify(r => r.GetByIdAsync(limitedFeature.Id, cts.Token), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle with the MarketingAiPosts limited feature returns correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_MarketingAiPostsLimitedFeature_ReturnsCorrectProperties()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.MarketingAiPosts;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        // Act
        Result<FeatureResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("marketing_ai_posts", result.Value.Key);
        Assert.AreEqual("Number of AI-generated marketing posts the organization can create", result.Value.Description);
    }

    /// <summary>
    /// Verifies that Handle with the ServicesPerBranch limited feature returns correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_ServicesPerBranchLimitedFeature_ReturnsCorrectProperties()
    {
        // Arrange
        LimitedFeature limitedFeature = LimitedFeature.ServicesPerBranch;
        var query = new GetLimitedFeatureByIdQuery(limitedFeature.Id);

        _limitedFeatureRepoMock.Setup(r => r.GetByIdAsync(limitedFeature.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        // Act
        Result<FeatureResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("services_per_branch", result.Value.Key);
        Assert.AreEqual("Number of services allowed per branch", result.Value.Description);
    }
}
