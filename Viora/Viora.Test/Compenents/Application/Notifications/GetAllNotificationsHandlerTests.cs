using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Notifications.GetAllNotifications;
using Viora.Domain.Abstractions;
using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;

namespace Viora.Test.Compenents.Application.Notifications;

[TestClass]
public sealed class GetAllNotificationsHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly GetAllNotificationsHandler _handler;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public GetAllNotificationsHandlerTests()
    {
        _handler = new GetAllNotificationsHandler(
            _userContextMock.Object,
            _notificationRepoMock.Object);
    }

    [TestMethod]
    public async Task Handle_UserHasNotifications_ReturnsList()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            Notification.Create(userId, new Title("Title 1"), new Body("Body 1"), FixedNow),
            Notification.Create(userId, new Title("Title 2"), new Body("Body 2"), FixedNow),
        };

        _userContextMock.Setup(c => c.UserId).Returns(userId);
        _notificationRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        Result<IEnumerable<Notification>> result = await _handler.Handle(
            new GetAllNotificationsQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count());
    }

    [TestMethod]
    public async Task Handle_UserHasNoNotifications_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        _userContextMock.Setup(c => c.UserId).Returns(userId);
        _notificationRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>());

        Result<IEnumerable<Notification>> result = await _handler.Handle(
            new GetAllNotificationsQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());
    }
}
