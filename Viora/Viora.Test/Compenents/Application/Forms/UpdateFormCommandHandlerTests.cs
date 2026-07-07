using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.UpdateForm;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the UpdateFormCommandHandler covering successful update and not-found scenarios.
/// </summary>
[TestClass]
public sealed class UpdateFormCommandHandlerTests
{
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateFormCommandHandler _handler;

    public UpdateFormCommandHandlerTests()
    {
        _handler = new UpdateFormCommandHandler(
            _formRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_FormNotFound_ThrowsNotFoundException()
    {
        Guid formId = Guid.NewGuid();
        var newFields = JsonDocument.Parse("{\"questions\":[]}");

        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new UpdateFormCommand(formId, newFields), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormExists_UpdatesFields()
    {
        Guid formId = Guid.NewGuid();
        var newFields = JsonDocument.Parse("{\"questions\":[{\"type\":\"text\"}]}");
        var form = CreateTestForm(formId);

        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new UpdateFormCommand(formId, newFields), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_FormExists_CallsSaveChangesAsync()
    {
        Guid formId = Guid.NewGuid();
        var newFields = JsonDocument.Parse("{}");

        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestForm(formId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(new UpdateFormCommand(formId, newFields), CancellationToken.None);

        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===== Helpers =====

    private static Form CreateTestForm(Guid? id = null)
    {
        return Form.Create(Guid.NewGuid(), null, "TestForm", JsonDocument.Parse("{}")).Value;
    }
}
