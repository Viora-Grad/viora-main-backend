using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.GetServiceForm;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the GetServiceFormQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetServiceFormQueryHandlerTests
{
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly GetServiceFormQueryHandler _handler;

    public GetServiceFormQueryHandlerTests()
    {
        _handler = new GetServiceFormQueryHandler(_formRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_ServiceHasNoForm_ThrowsNotFoundException()
    {
        Guid serviceId = Guid.NewGuid();
        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetServiceFormQuery(serviceId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ServiceHasForm_ReturnsFormResponse()
    {
        Guid serviceId = Guid.NewGuid();
        var form = CreateTestForm(serviceId);

        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);

        var result = await _handler.Handle(
            new GetServiceFormQuery(serviceId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(serviceId, result.Value.ServiceId);
        Assert.AreEqual("TestForm", result.Value.Name);
    }

    [TestMethod]
    public async Task Handle_FormWithStaffId_ReturnsCorrectStaffId()
    {
        Guid serviceId = Guid.NewGuid();
        Guid staffId = Guid.NewGuid();
        var form = Form.Create(serviceId, staffId, "StaffForm", JsonDocument.Parse("{}")).Value;

        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);

        var result = await _handler.Handle(
            new GetServiceFormQuery(serviceId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(staffId, result.Value.StaffId);
    }

    // ===== Helpers =====

    private static Form CreateTestForm(Guid serviceId)
    {
        return Form.Create(serviceId, null, "TestForm", JsonDocument.Parse("{}")).Value;
    }
}
