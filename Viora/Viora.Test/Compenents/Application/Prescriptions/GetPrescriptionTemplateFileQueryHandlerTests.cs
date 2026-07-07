using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Prescriptions.GetPrescriptionFile;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the GetPrescriptionTemplateFileQueryHandler covering successful retrieval, not-found, and null file scenarios.
/// </summary>
[TestClass]
public sealed class GetPrescriptionTemplateFileQueryHandlerTests
{
    private readonly Mock<IPrescriptionTemplateRepository> _templateRepoMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly GetPrescriptionTemplateFileQueryHandler _handler;

    public GetPrescriptionTemplateFileQueryHandlerTests()
    {
        _handler = new GetPrescriptionTemplateFileQueryHandler(
            _templateRepoMock.Object,
            _storageServiceMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_TemplateNotFound_ThrowsNotFoundException()
    {
        Guid templateId = Guid.NewGuid();
        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PrescriptionTemplate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPrescriptionTemplateFileQuery(templateId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_TemplateHasNoFile_ReturnsFailure()
    {
        Guid templateId = Guid.NewGuid();
        var template = CreateTestTemplateWithoutFile(templateId);

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(
            new GetPrescriptionTemplateFileQuery(templateId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(PrescriptionError.PrescriptionTemplateNotFound.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_TemplateHasFile_ReturnsStream()
    {
        Guid templateId = Guid.NewGuid();
        var media = MediaFile.Create("template.pdf", 1024, "key/template.pdf", "application/pdf", DateTime.UtcNow, 10_000_000, Guid.NewGuid()).Value;
        var template = CreateTestTemplateWithFile(templateId, media);

        var stream = new MemoryStream(new byte[1024]);
        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _storageServiceMock.Setup(s => s.GetFileStream("key/template.pdf")).Returns(stream);

        var result = await _handler.Handle(
            new GetPrescriptionTemplateFileQuery(templateId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(stream, result.Value.Content);
        Assert.AreEqual("application/pdf", result.Value.ContentType);
    }

    // ===== Helpers =====

    private static PrescriptionTemplate CreateTestTemplateWithoutFile(Guid id)
    {
        return new PrescriptionTemplate(id, Guid.NewGuid(), "TestTemplate", null, 1.0, 1.0, 1.0, 1.0);
    }

    private static PrescriptionTemplate CreateTestTemplateWithFile(Guid id, MediaFile media)
    {
        var template = new PrescriptionTemplate(id, Guid.NewGuid(), "TestTemplate", media.Id, 1.0, 1.0, 1.0, 1.0);
        SetTemplateFile(template, media);
        return template;
    }

    private static void SetTemplateFile(PrescriptionTemplate template, MediaFile media)
    {
        typeof(PrescriptionTemplate).GetField("File")!.SetValue(template, media);
    }
}
