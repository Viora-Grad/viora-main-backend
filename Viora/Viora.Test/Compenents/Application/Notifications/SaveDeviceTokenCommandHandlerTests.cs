using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Notifications.NotificationService;
using Viora.Application.Notifications.SaveDeviceToken;
using Viora.Domain.Abstractions;

namespace Viora.Test.Compenents.Application.Notifications;

[TestClass]
public sealed class SaveDeviceTokenCommandHandlerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly SaveDeviceTokenCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid NotificationId = Guid.NewGuid();

    public SaveDeviceTokenCommandHandlerTests()
    {
        _handler = new SaveDeviceTokenCommandHandler(
            _notificationServiceMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidToken_ReturnsNotificationId()
    {
        const string token = "device-token-value";
        var command = new SaveDeviceTokenCommand(token);

        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _notificationServiceMock.Setup(s => s.SaveDeviceTokenAsync(
                UserId, token, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(NotificationId));

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(NotificationId, result.Value);
    }

}
