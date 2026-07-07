using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.GetFormSubmissionById;
using Viora.Application.Forms.Shared;
using Viora.Domain.Forms;
using Viora.Domain.Medias;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the GetFormSubmissionByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetFormSubmissionByIdQueryHandlerTests
{
    private readonly Mock<IFormSubmissionRepository> _formSubmissionRepoMock = new();
    private readonly Mock<IFormSubmissionMediaRepository> _formSubmissionMediaRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IMediaRepository> _mediaRepoMock = new();
    private readonly GetFormSubmissionByIdQueryHandler _handler;

    public GetFormSubmissionByIdQueryHandlerTests()
    {
        _handler = new GetFormSubmissionByIdQueryHandler(
            _formSubmissionRepoMock.Object,
            _formSubmissionMediaRepoMock.Object,
            _dateTimeProviderMock.Object,
            _mediaRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_SubmissionNotFound_ThrowsNotFoundException()
    {
        Guid submissionId = Guid.NewGuid();
        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FormSubmission?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetFormSubmissionByIdQuery(submissionId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoMediaFiles_ReturnsEmptyFileList()
    {
        var submission = CreateTestFormSubmission();
        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _formSubmissionMediaRepoMock.Setup(r => r.GetByFormSubmissionIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormSubmissionMedia>());
        _mediaRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MediaFile>());

        var result = await _handler.Handle(
            new GetFormSubmissionByIdQuery(submission.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(submission.Id, result.Value.Id);
        Assert.AreEqual(0, result.Value.FileList.Count);
    }

    [TestMethod]
    public async Task Handle_SubmissionWithMedia_ReturnsResponseWithMedia()
    {
        var submission = CreateTestFormSubmission();
        var mediaFile = CreateTestMediaFile();
        var submissionMedia = FormSubmissionMedia.Create(submission.Id, mediaFile.Id).Value;

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _formSubmissionMediaRepoMock.Setup(r => r.GetByFormSubmissionIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormSubmissionMedia> { submissionMedia });
        _mediaRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MediaFile> { mediaFile });

        var result = await _handler.Handle(
            new GetFormSubmissionByIdQuery(submission.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(submission.Id, result.Value.Id);
        Assert.AreEqual(1, result.Value.FileList.Count);
    }

    // ===== Helpers =====

    private static FormSubmission CreateTestFormSubmission()
    {
        return FormSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), JsonDocument.Parse("{}"), DateTime.UtcNow).Value;
    }

    private static MediaFile CreateTestMediaFile()
    {
        return MediaFile.Create("test.png", 1024, "key/test.png", "image/png", DateTime.UtcNow, 10_000_000, Guid.NewGuid()).Value;
    }
}
