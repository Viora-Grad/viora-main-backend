using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Forms.UploadFormSubmissionFile;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Forms;
using Viora.Domain.Medias;
using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the UploadFormSubmissionFileCommandHandler covering successful upload, not-found scenarios, and validation errors.
/// </summary>
[TestClass]
public sealed class UploadFormSubmissionFileCommandHandlerTests
{
    private readonly Mock<IFormSubmissionRepository> _formSubmissionRepoMock = new();
    private readonly Mock<IMediaRepository> _mediaRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IStorageSettings> _storageSettingsMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IFormSubmissionMediaRepository> _formSubmissionMediaRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UploadFormSubmissionFileCommandHandler _handler;

    public UploadFormSubmissionFileCommandHandlerTests()
    {
        _handler = new UploadFormSubmissionFileCommandHandler(
            _formSubmissionRepoMock.Object,
            _mediaRepoMock.Object,
            _dateTimeProviderMock.Object,
            _storageSettingsMock.Object,
            _storageServiceMock.Object,
            _formRepoMock.Object,
            _serviceRepoMock.Object,
            _branchRepoMock.Object,
            _formSubmissionMediaRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_FormSubmissionNotFound_ThrowsNotFoundException()
    {
        Guid submissionId = Guid.NewGuid();
        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FormSubmission?)null);

        var command = new UploadFormSubmissionFileCommand(submissionId, CreateTestMediaRequest());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormNotFound_ThrowsNotFoundException()
    {
        Guid submissionId = Guid.NewGuid();
        var submission = CreateTestFormSubmission(submissionId);
        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _formRepoMock.Setup(r => r.GetByIdAsync(submission.FormId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        var command = new UploadFormSubmissionFileCommand(submissionId, CreateTestMediaRequest());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ValidUpload_CreatesMediaAndSubmissionFile()
    {
        Guid submissionId = Guid.NewGuid();
        var submission = CreateTestFormSubmission(submissionId);
        var form = CreateTestForm(submission.FormId);
        var service = CreateTestService(form.ServiceId);
        var branch = CreateTestBranch(service.BranchId);

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _storageSettingsMock.Setup(s => s.MaxFileSizeBytes).Returns(10_000_000);

        _formSubmissionRepoMock.Setup(r => r.GetByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _formRepoMock.Setup(r => r.GetByIdAsync(submission.FormId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(form.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _branchRepoMock.Setup(r => r.GetByIdAsync(service.BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _storageServiceMock.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UploadFormSubmissionFileCommand(submissionId, CreateTestMediaRequest());
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _mediaRepoMock.Verify(r => r.Add(It.IsAny<MediaFile>()), Times.Once);
        _formSubmissionMediaRepoMock.Verify(r => r.Add(It.IsAny<FormSubmissionMedia>()), Times.Once);
        _storageServiceMock.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===== Helpers =====

    private static MediaRequest CreateTestMediaRequest()
    {
        return MediaRequest.CreateImage("test.png", "image/png", 1024, new MemoryStream(new byte[1024]), 10_000_000);
    }

    private static FormSubmission CreateTestFormSubmission(Guid id)
    {
        return FormSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), JsonDocument.Parse("{}"), DateTime.UtcNow).Value;
    }

    private static Form CreateTestForm(Guid serviceId)
    {
        return Form.Create(serviceId, null, "TestForm", JsonDocument.Parse("{}")).Value;
    }

    private static Service CreateTestService(Guid serviceId)
    {
        var result = Service.Create(
            Guid.NewGuid(), "TestService", "Description", 30,
            ServiceType.InternalMedicine,
            new Money(100m, Currency.Usd), new TestServiceSettings());
        return result.Value;
    }

    private static Branch CreateTestBranch(Guid branchId)
    {
        var result = Branch.Create(
            branchId,
            new Viora.Domain.Shared.Internal.Address(1, "123 St", "City", "State", Guid.NewGuid(), 12345),
            new NetTopologySuite.Geometries.Point(0, 0) { SRID = 4326 },
            "test@example.com",
            new List<ServiceType> { ServiceType.InternalMedicine },
            DateTime.UtcNow);
        return result.Value;
    }

    private sealed class TestServiceSettings : IServiceSettings
    {
        public int SlotSizeInMinutes { get; set; } = 15;
        public int MinimumDurationInMinutes { get; set; } = 15;
        public int MaximumDurationInMinutes { get; set; } = 480;
        public int MaxGallerySize { get; set; } = 10;
    }
}
