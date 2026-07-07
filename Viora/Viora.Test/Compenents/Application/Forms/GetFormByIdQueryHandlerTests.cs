using Moq;
using System.Reflection;
using System.Text.Json;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.GetForm;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the GetFormByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetFormByIdQueryHandlerTests
{
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly GetFormByIdQueryHandler _handler;

    public GetFormByIdQueryHandlerTests()
    {
        _handler = new GetFormByIdQueryHandler(_formRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_FormNotFound_ThrowsNotFoundException()
    {
        Guid formId = Guid.NewGuid();
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetFormByIdQuery(formId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormExists_ReturnsFormResponse()
    {
        var form = CreateTestForm(Guid.NewGuid());
        _formRepoMock.Setup(r => r.GetByIdAsync(form.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);

        var result = await _handler.Handle(
            new GetFormByIdQuery(form.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(form.Id, result.Value.Id);
        Assert.AreEqual(form.ServiceId, result.Value.ServiceId);
        Assert.AreEqual(form.StaffId, result.Value.StaffId);
        Assert.AreEqual("TestForm", result.Value.Name);
    }

    [TestMethod]
    public async Task Handle_FormWithNullStaffId_ReturnsNullStaffId()
    {
        var form = Form.Create(Guid.NewGuid(), null, "TestForm", JsonDocument.Parse("{}")).Value;

        _formRepoMock.Setup(r => r.GetByIdAsync(form.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);

        var result = await _handler.Handle(
            new GetFormByIdQuery(form.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.StaffId);
    }

    // ===== Helpers =====

    private static Form CreateTestForm(Guid serviceId)
    {
        return Form.Create(serviceId, null, "TestForm", JsonDocument.Parse("{}")).Value;
    }
}
