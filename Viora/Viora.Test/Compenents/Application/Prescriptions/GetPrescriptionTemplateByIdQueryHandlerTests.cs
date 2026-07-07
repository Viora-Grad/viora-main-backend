using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Prescriptions.Shared;
using Viora.Application.Prescriptions.GetTemplateById;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the GetPrescriptionTemplateByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetPrescriptionTemplateByIdQueryHandlerTests
{
    private readonly Mock<IPrescriptionTemplateRepository> _templateRepoMock = new();
    private readonly GetPrescriptionTemplateByIdQueryHandler _handler;

    public GetPrescriptionTemplateByIdQueryHandlerTests()
    {
        _handler = new GetPrescriptionTemplateByIdQueryHandler(_templateRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_TemplateNotFound_ThrowsNotFoundException()
    {
        Guid templateId = Guid.NewGuid();
        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PrescriptionTemplate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPrescriptionTemplateByIdQuery(templateId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_TemplateExists_ReturnsResponse()
    {
        Guid templateId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        var template = CreateTestTemplate(templateId, orgId);

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(
            new GetPrescriptionTemplateByIdQuery(templateId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(templateId, result.Value.Id);
        Assert.AreEqual(orgId, result.Value.OrganizaionId);
        Assert.AreEqual("TestTemplate", result.Value.Name);
        Assert.AreEqual(1.0, result.Value.TopMargin);
    }

    [TestMethod]
    public async Task Handle_TemplateWithMedia_ReturnsMediaResponse()
    {
        Guid templateId = Guid.NewGuid();
        var template = CreateTestTemplate(templateId, Guid.NewGuid());

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(
            new GetPrescriptionTemplateByIdQuery(templateId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value.Media);
    }

    // ===== Helpers =====

    private static PrescriptionTemplate CreateTestTemplate(Guid id, Guid orgId)
    {
        var media = MediaFile.Create("template.pdf", 1024, "key/template.pdf", "application/pdf", DateTime.UtcNow, 10_000_000, orgId).Value;
        var template = new PrescriptionTemplate(id, orgId, "TestTemplate", media.Id, 1.0, 1.0, 1.0, 1.0);
        SetTemplateFile(template, media);
        return template;
    }

    private static void SetTemplateFile(PrescriptionTemplate template, MediaFile media)
    {
        typeof(PrescriptionTemplate).GetField("File")!.SetValue(template, media);
    }
}
