using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.DeleteForm;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the DeleteFormCommandHandler covering successful deletion and not-found scenarios.
/// </summary>
[TestClass]
public sealed class DeleteFormCommandHandlerTests
{
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeleteFormCommandHandler _handler;

    public DeleteFormCommandHandlerTests()
    {
        _handler = new DeleteFormCommandHandler(
            _formRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_FormNotFound_ThrowsNotFoundException()
    {
        Guid formId = Guid.NewGuid();
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteFormCommand(formId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormExists_DeletesForm()
    {
        Guid formId = Guid.NewGuid();
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestForm(formId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new DeleteFormCommand(formId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _formRepoMock.Verify(r => r.Remove(formId), Times.Once);
        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_FormExists_CallsRepositoryWithCorrectId()
    {
        Guid formId = Guid.NewGuid();
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestForm(formId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(new DeleteFormCommand(formId), CancellationToken.None);

        _formRepoMock.Verify(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()), Times.Once);
        _formRepoMock.Verify(r => r.Remove(formId), Times.Once);
    }

    // ===== Helpers =====

    private static Form CreateTestForm(Guid? id = null)
    {
        return Form.Create(Guid.NewGuid(), null, "TestForm", JsonDocument.Parse("{}")).Value;
    }
}
