using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Prescriptions.CreatePrescriptionTemplate;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Prescriptions;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the CreatePrescriptionTemplateCommandHandler covering successful creation, organization not found, and file handling.
/// </summary>
[TestClass]
public sealed class CreatePrescriptionTemplateCommandHandlerTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IStorageSettings> _storageSettingsMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IPrescriptionTemplateRepository> _templateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMediaRepository> _mediaRepoMock = new();
    private readonly CreatePrescriptionTemplateCommandHandler _handler;

    public CreatePrescriptionTemplateCommandHandlerTests()
    {
        _handler = new CreatePrescriptionTemplateCommandHandler(
            _organizationRepoMock.Object,
            _dateTimeProviderMock.Object,
            _storageSettingsMock.Object,
            _storageServiceMock.Object,
            _templateRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediaRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var command = new CreatePrescriptionTemplateCommand(orgId, "Template", null, 1.0, 1.0, 1.0, 1.0);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_TemplateWithNoFile_CreatesTemplateWithoutMedia()
    {
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreatePrescriptionTemplateCommand(orgId, "Template", null, 1.0, 1.0, 1.0, 1.0);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _templateRepoMock.Verify(r => r.Add(It.IsAny<PrescriptionTemplate>()), Times.Once);
        _mediaRepoMock.Verify(r => r.Add(It.IsAny<MediaFile>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_TemplateWithFile_CreatesTemplateWithMedia()
    {
        Guid orgId = Guid.NewGuid();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _storageSettingsMock.Setup(s => s.MaxFileSizeBytes).Returns(10_000_000);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _storageServiceMock.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreatePrescriptionTemplateCommand(orgId, "Template", CreateTestMediaRequest(), 1.0, 1.0, 1.0, 1.0);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _mediaRepoMock.Verify(r => r.Add(It.IsAny<MediaFile>()), Times.Once);
        _storageServiceMock.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ValidTemplate_SavesChanges()
    {
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreatePrescriptionTemplateCommand(orgId, "Template", null, 1.0, 1.0, 1.0, 1.0);

        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "About", "Service description",
            new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow,
            ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

    private static MediaRequest CreateTestMediaRequest()
    {
        return MediaRequest.CreateDocument("template.pdf", "application/pdf", 1024, new MemoryStream(new byte[1024]), 10_000_000);
    }
}
