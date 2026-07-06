using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Notifications.GetNotification;
using Viora.Domain.Abstractions;
using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;

namespace Viora.Test.Compenents.Application.Notifications;

[TestClass]
public sealed class GetNotificationQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly GetNotificationQueryHandler _handler;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public GetNotificationQueryHandlerTests()
    {
        _handler = new GetNotificationQueryHandler(_notificationRepoMock.Object);
    }

    [TestMethod]
    public async Task Handle_NotificationFound_ReturnsResponse()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), new Title("Test Title"), new Body("Test Body"), FixedNow);
        var query = new GetNotificationQuery(notification.Id);

        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        Result<Notification> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(notification.Id, result.Value.Id);
    }

    [TestMethod]
    public async Task Handle_NotificationNotFound_ThrowsNotFoundException()
    {
        var query = new GetNotificationQuery(Guid.NewGuid());

        _notificationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
