using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Marketing.Abstractions;
using Viora.Application.Marketing.PollContent;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Test.Marketing;

// Verifies the async two-step poll: pending while Manus runs; on completion the copy becomes the draft idea
// and the pending task is cleared.
[TestClass]
public sealed class PollContentCommandHandlerTests
{
    private readonly Mock<IMarketingChatSessionRepository> _sessions = new();
    private readonly Mock<IManusClient> _manus = new();
    private readonly Mock<IUserContext> _userContext = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc);

    private PollContentCommandHandler CreateHandler() => new(
        _sessions.Object, _manus.Object, _userContext.Object, _clock.Object, _unitOfWork.Object,
        NullLogger<PollContentCommandHandler>.Instance);

    private MarketingChatSession ArrangePendingSession()
    {
        var session = MarketingChatSession.Create(_orgId, _userId, _now);
        session.SetPendingManusTask("task-1", "https://manus.im/app/task-1", _now);

        _userContext.Setup(u => u.OrganizationId).Returns(_orgId);
        _clock.Setup(c => c.UtcNow).Returns(_now);
        _sessions.Setup(s => s.GetByIdWithMessagesAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return session;
    }

    [TestMethod]
    public async Task Task_still_running_returns_pending_and_does_not_save()
    {
        var session = ArrangePendingSession();
        _manus.Setup(m => m.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ManusTaskResult(Completed: false, Content: null)));

        var result = await CreateHandler().Handle(new PollContentCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Pending", result.Value.Status);
        Assert.IsNull(session.LatestManusIdea);
        Assert.AreEqual("task-1", session.PendingManusTaskId); // still pending
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Completed_task_stores_copy_clears_pending_and_returns_ready()
    {
        var session = ArrangePendingSession();
        _manus.Setup(m => m.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ManusTaskResult(Completed: true, Content: "Big summer sale copy!")));

        var result = await CreateHandler().Handle(new PollContentCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Ready", result.Value.Status);
        Assert.AreEqual("Big summer sale copy!", result.Value.Content);
        Assert.AreEqual("Big summer sale copy!", session.LatestManusIdea);
        Assert.IsNull(session.PendingManusTaskId); // cleared
        Assert.AreEqual(1, session.Messages.Count); // assistant message appended
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
