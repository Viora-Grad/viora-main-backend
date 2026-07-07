using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.CreateForm;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the CreateFormCommandHandler covering creation, staff validation, service validation, and form conflict scenarios.
/// </summary>
[TestClass]
public sealed class CreateFormCommandHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateFormCommandHandler _handler;

    public CreateFormCommandHandlerTests()
    {
        _handler = new CreateFormCommandHandler(
            _staffRepoMock.Object,
            _formRepoMock.Object,
            _serviceRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        Guid staffId = Guid.NewGuid();
        Guid serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("{}");

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateFormCommand(serviceId, staffId, "TestForm", fields), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
    {
        Guid serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("{}");

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateFormCommand(serviceId, null, "TestForm", fields), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormAlreadyExistsForService_ReturnsFailure()
    {
        Guid serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("{}");

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestService(serviceId));
        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestForm(serviceId));

        var result = await _handler.Handle(
            new CreateFormCommand(serviceId, null, "TestForm", fields), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FormError.FormConflict.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_StaffIdIsNull_SkipsStaffValidation()
    {
        Guid serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("{}");

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestService(serviceId));
        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateFormCommand(serviceId, null, "TestForm", fields), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _formRepoMock.Verify(r => r.Add(It.IsAny<Form>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ValidStaffAndService_CreatesForm()
    {
        Guid staffId = Guid.NewGuid();
        Guid serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("{}");

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestStaff());
        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestService(serviceId));
        _formRepoMock.Setup(r => r.GetServiceFormAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateFormCommand(serviceId, staffId, "TestForm", fields), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _formRepoMock.Verify(r => r.Add(It.IsAny<Form>()), Times.Once);
        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===== Helpers =====

    private static Staff CreateTestStaff()
    {
        return Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
    }

    private static Service CreateTestService(Guid serviceId)
    {
        var result = Service.Create(
            Guid.NewGuid(), "TestService", "Description", 30,
            ServiceType.InternalMedicine,
            new Money(100m, Currency.Usd), new TestServiceSettings());
        return result.Value;
    }

    private static Form CreateTestForm(Guid serviceId)
    {
        return Form.Create(serviceId, null, "TestForm", JsonDocument.Parse("{}")).Value;
    }

    private sealed class TestServiceSettings : IServiceSettings
    {
        public int SlotSizeInMinutes { get; set; } = 15;
        public int MinimumDurationInMinutes { get; set; } = 15;
        public int MaximumDurationInMinutes { get; set; } = 480;
        public int MaxGallerySize { get; set; } = 10;
    }
}
