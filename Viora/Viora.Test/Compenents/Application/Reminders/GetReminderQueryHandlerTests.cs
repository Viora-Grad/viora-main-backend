using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Reminders.GetReminder;
using Viora.Domain.Abstractions;
using Viora.Domain.Reminders;

namespace Viora.Test.Compenents.Application.Reminders;

[TestClass]
public sealed class GetReminderQueryHandlerTests
{
    private readonly Mock<IReminderRepository> _reminderRepoMock = new();
    private readonly GetReminderQueryHandler _handler;

    public GetReminderQueryHandlerTests()
    {
        _handler = new GetReminderQueryHandler(_reminderRepoMock.Object);
    }

    [TestMethod]
    public async Task Handle_ReminderFound_ReturnsReminder()
    {
        var reminder = Reminder.Create(
            Guid.NewGuid(), "Title", "Body",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var query = new GetReminderQuery(reminder.Id);

        _reminderRepoMock.Setup(r => r.GetByIdAsync(reminder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reminder);

        Result<Reminder> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(reminder.Id, result.Value.Id);
    }

    [TestMethod]
    public async Task Handle_ReminderNotFound_ThrowsNotFoundException()
    {
        var query = new GetReminderQuery(Guid.NewGuid());

        _reminderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reminder?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
