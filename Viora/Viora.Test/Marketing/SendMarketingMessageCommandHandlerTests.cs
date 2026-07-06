using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Marketing.Abstractions;
using Viora.Application.Marketing.FinalizePost;
using Viora.Application.Marketing.SendMessage;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Test.Marketing;

// Verifies the orchestrator routes to the correct branch. Two intents only.
[TestClass]
public sealed class SendMarketingMessageCommandHandlerTests
{
    private readonly Mock<IMarketingChatSessionRepository> _sessions = new();
    private readonly Mock<IMetaPageCredentialRepository> _credentials = new();
    private readonly Mock<IMarketingIntentDetector> _intent = new();
    private readonly Mock<IManusClient> _manus = new();
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IUserContext> _userContext = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc);

    private SendMarketingMessageCommandHandler CreateHandler() => new(
        _sessions.Object, _credentials.Object, _intent.Object, _manus.Object, _sender.Object,
        _userContext.Object, _clock.Object, _unitOfWork.Object,
        NullLogger<SendMarketingMessageCommandHandler>.Instance);

    private MarketingChatSession ArrangeSession(bool withDraftIdea)
    {
        var session = MarketingChatSession.Create(_orgId, _userId, _now);
        if (withDraftIdea)
            session.SetManusIdea("a summer sale promo idea", null, _now);

        _userContext.Setup(u => u.OrganizationId).Returns(_orgId);
        _clock.Setup(c => c.UtcNow).Returns(_now);
        _sessions.Setup(s => s.GetByIdWithMessagesAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return session;
    }

    [TestMethod]
    public async Task MarketingContent_intent_starts_Manus_task_and_returns_pending()
    {
        var session = ArrangeSession(withDraftIdea: false);
        _intent.Setup(i => i.DetectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MarketingIntent.MarketingContent);
        _manus.Setup(m => m.CreateTaskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ManusTaskRef("task-1", "https://manus.im/app/task-1")));

        var result = await CreateHandler().Handle(
            new SendMarketingMessageCommand(session.Id, "write me a promo"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.ContentPending);
        Assert.AreEqual(MarketingIntent.MarketingContent.ToString(), result.Value.DetectedIntent);
        Assert.AreEqual("task-1", session.PendingManusTaskId);

        _manus.Verify(m => m.CreateTaskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(s => s.Send(It.IsAny<FinalizePostCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task FinalizePost_intent_dispatches_FinalizePostCommand_and_does_not_call_Manus()
    {
        var session = ArrangeSession(withDraftIdea: true);
        _intent.Setup(i => i.DetectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MarketingIntent.FinalizePost);
        _credentials.Setup(c => c.GetActiveByOrganizationAsync(_orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetaPageCredential.Create(_orgId, "page-1", "encrypted", _now));
        _sender.Setup(s => s.Send(It.IsAny<FinalizePostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new FinalizePostResult(session.Id, "page-1_post-9", "Title", "Archived on your Page.")));

        var result = await CreateHandler().Handle(
            new SendMarketingMessageCommand(session.Id, "go ahead and post it"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(MarketingPostStatus.Archived.ToString(), result.Value.Status);
        Assert.AreEqual("page-1_post-9", result.Value.FacebookPostId);

        _sender.Verify(s => s.Send(It.IsAny<FinalizePostCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _manus.Verify(m => m.CreateTaskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task FinalizePost_without_credential_returns_error_and_never_dispatches()
    {
        var session = ArrangeSession(withDraftIdea: true);
        _intent.Setup(i => i.DetectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MarketingIntent.FinalizePost);
        _credentials.Setup(c => c.GetActiveByOrganizationAsync(_orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetaPageCredential?)null);

        var result = await CreateHandler().Handle(
            new SendMarketingMessageCommand(session.Id, "post it now"), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.CredentialNotFound, result.Error);
        // Fail-fast: the quota-consuming finalize command must not be dispatched.
        _sender.Verify(s => s.Send(It.IsAny<FinalizePostCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
