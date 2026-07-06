using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Marketing.Abstractions;
using Viora.Application.Marketing.FinalizePost;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Test.Marketing;

// Finalize prepares a LOCAL draft (Groq builds the copy, it's stored + the session is Archived). No Facebook
// call happens here. Quota timing: the single success-only save commits the draft (and the pipeline's -1).
[TestClass]
public sealed class FinalizePostCommandHandlerTests
{
    private readonly Mock<IMarketingChatSessionRepository> _sessions = new();
    private readonly Mock<IMarketingPostJsonBuilder> _builder = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);

    private FinalizePostCommandHandler CreateHandler() => new(
        _sessions.Object, _builder.Object, _clock.Object, _unitOfWork.Object,
        NullLogger<FinalizePostCommandHandler>.Instance);

    private MarketingChatSession ArrangeDraftSession()
    {
        var session = MarketingChatSession.Create(_orgId, _userId, _now);
        session.SetManusIdea("a summer sale promo idea", null, _now);

        _clock.Setup(c => c.UtcNow).Returns(_now);
        _sessions.Setup(s => s.GetByIdWithMessagesAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return session;
    }

    [TestMethod]
    public async Task Success_stores_draft_archives_and_saves_once()
    {
        var session = ArrangeDraftSession();
        _builder.Setup(b => b.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new GeneratedPost("Title", "Post body", "https://shop.example.com")));

        var result = await CreateHandler().Handle(new FinalizePostCommand(session.Id, _orgId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(MarketingPostStatus.Archived, session.Status);
        Assert.AreEqual("Post body", session.PostMessage);
        Assert.AreEqual("https://shop.example.com", session.PostLink);
        Assert.AreEqual("Title", session.Title);
        Assert.IsNull(session.FacebookPostId); // nothing created on Facebook yet
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Builder_failure_returns_error_and_does_not_save()
    {
        var session = ArrangeDraftSession();
        _builder.Setup(b => b.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<GeneratedPost>(MarketingErrors.ContentGenerationFailed));

        var result = await CreateHandler().Handle(new FinalizePostCommand(session.Id, _orgId), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.ContentGenerationFailed, result.Error);
        Assert.AreEqual(MarketingPostStatus.Draft, session.Status); // unchanged
        // No save -> the pipeline's quota -1 is rolled back.
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task No_generated_content_is_rejected()
    {
        var session = MarketingChatSession.Create(_orgId, _userId, _now); // never generated any content
        _clock.Setup(c => c.UtcNow).Returns(_now);
        _sessions.Setup(s => s.GetByIdWithMessagesAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var result = await CreateHandler().Handle(new FinalizePostCommand(session.Id, _orgId), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.NoDraftContent, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
