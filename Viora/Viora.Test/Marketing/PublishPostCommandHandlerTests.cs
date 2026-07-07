using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Security;
using Viora.Application.Marketing.Abstractions;
using Viora.Application.Marketing.PublishPost;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Test.Marketing;

// Publish creates the LIVE post in one shot from the stored draft: a feed post for text/link, or a native
// photo post (download image, upload as source) when the chat generated an image.
[TestClass]
public sealed class PublishPostCommandHandlerTests
{
    private readonly Mock<IMarketingChatSessionRepository> _sessions = new();
    private readonly Mock<IMetaPageCredentialRepository> _credentials = new();
    private readonly Mock<IMetaGraphService> _meta = new();
    private readonly Mock<IManusClient> _manus = new();
    private readonly Mock<ICipher> _cipher = new();
    private readonly Mock<IUserContext> _userContext = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);

    private PublishPostCommandHandler CreateHandler() => new(
        _sessions.Object, _credentials.Object, _meta.Object, _manus.Object, _cipher.Object,
        _userContext.Object, _clock.Object, _unitOfWork.Object, NullLogger<PublishPostCommandHandler>.Instance);

    private MarketingChatSession ArrangeArchivedSession(string? imageUrl = null)
    {
        var session = MarketingChatSession.Create(_orgId, _userId, _now);
        session.SetManusIdea("idea", imageUrl, null, _now);
        session.MarkArchived("Post body", "https://shop.example.com", "Title", _now);

        _userContext.Setup(u => u.OrganizationId).Returns(_orgId);
        _clock.Setup(c => c.UtcNow).Returns(_now);
        _sessions.Setup(s => s.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _credentials.Setup(c => c.GetActiveByOrganizationAsync(_orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetaPageCredential.Create(_orgId, "page-1", "encrypted", _now));
        _cipher.Setup(c => c.Decrypt("encrypted")).Returns("plain-token");
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return session;
    }

    [TestMethod]
    public async Task Text_draft_publishes_via_feed()
    {
        var session = ArrangeArchivedSession();
        _meta.Setup(m => m.CreatePostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MetaPostPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new MetaPostResult("page-1_post-9")));

        var result = await CreateHandler().Handle(new PublishPostCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(MarketingPostStatus.Published, session.Status);
        Assert.AreEqual("page-1_post-9", session.FacebookPostId);
        _meta.Verify(m => m.CreatePhotoPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Image_draft_downloads_and_publishes_via_photos()
    {
        var session = ArrangeArchivedSession(imageUrl: "https://api.manus.ai/files/img-1.png");
        _manus.Setup(m => m.DownloadImageAsync("https://api.manus.ai/files/img-1.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ManusImage([1, 2, 3], "image/png", "img-1.png")));
        _meta.Setup(m => m.CreatePhotoPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new MetaPostResult("page-1_photo-post-7")));

        var result = await CreateHandler().Handle(new PublishPostCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(MarketingPostStatus.Published, session.Status);
        Assert.AreEqual("page-1_photo-post-7", session.FacebookPostId);
        _manus.Verify(m => m.DownloadImageAsync("https://api.manus.ai/files/img-1.png", It.IsAny<CancellationToken>()), Times.Once);
        _meta.Verify(m => m.CreatePostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MetaPostPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Meta_failure_returns_error_and_does_not_publish()
    {
        var session = ArrangeArchivedSession();
        _meta.Setup(m => m.CreatePostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MetaPostPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed));

        var result = await CreateHandler().Handle(new PublishPostCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.MetaGraphFailed, result.Error);
        Assert.AreEqual(MarketingPostStatus.Archived, session.Status); // stays archived for retry
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Missing_credential_returns_error()
    {
        var session = ArrangeArchivedSession();
        _credentials.Setup(c => c.GetActiveByOrganizationAsync(_orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetaPageCredential?)null);

        var result = await CreateHandler().Handle(new PublishPostCommand(session.Id), CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.CredentialNotFound, result.Error);
    }
}
