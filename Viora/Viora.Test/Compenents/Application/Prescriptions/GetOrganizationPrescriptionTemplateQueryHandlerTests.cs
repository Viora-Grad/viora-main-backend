using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Prescriptions.Shared;
using Viora.Application.Prescriptions.GetOrganizationPrescroptionTemplate;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Prescriptions;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the GetOrganizationPrescriptionTemplateQueryHandler covering successful retrieval, not-found, and empty results.
/// </summary>
[TestClass]
public sealed class GetOrganizationPrescriptionTemplateQueryHandlerTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<IPrescriptionTemplateRepository> _templateRepoMock = new();
    private readonly GetOrganizationPrescriptionTemplateQueryHandler _handler;

    public GetOrganizationPrescriptionTemplateQueryHandlerTests()
    {
        _handler = new GetOrganizationPrescriptionTemplateQueryHandler(
            _organizationRepoMock.Object,
            _templateRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetOrganizaionPrescriptionTamplateQuery(orgId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoTemplates_ReturnsFailure()
    {
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _templateRepoMock.Setup(r => r.GetByOrganizationAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrescriptionTemplate>());

        var result = await _handler.Handle(
            new GetOrganizaionPrescriptionTamplateQuery(orgId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(PrescriptionError.PrescriptionTemplateNotFound.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_TemplatesExist_ReturnsList()
    {
        var org = CreateTestOrganization(Guid.NewGuid());
        var template = CreateTestTemplate(Guid.NewGuid(), org.Id);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _templateRepoMock.Setup(r => r.GetByOrganizationAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrescriptionTemplate> { template });

        var result = await _handler.Handle(
            new GetOrganizaionPrescriptionTamplateQuery(org.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Count);
        Assert.AreEqual("TestTemplate", result.Value[0].Name);
    }

    [TestMethod]
    public async Task Handle_MultipleTemplates_ReturnsAll()
    {
        var org = CreateTestOrganization(Guid.NewGuid());
        var template1 = CreateTestTemplate(Guid.NewGuid(), org.Id);
        var template2 = CreateTestTemplate(Guid.NewGuid(), org.Id);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _templateRepoMock.Setup(r => r.GetByOrganizationAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrescriptionTemplate> { template1, template2 });

        var result = await _handler.Handle(
            new GetOrganizaionPrescriptionTamplateQuery(org.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "About", "Service description",
            new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow,
            ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

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
