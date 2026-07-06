using Moq;
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.UpdateServices;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class UpdateServicesCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceSettings> _serviceSettingsMock = new();
    private readonly UpdateServicesCommandHandler _handler;

    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public UpdateServicesCommandHandlerTests()
    {
        _handler = new UpdateServicesCommandHandler(
            _branchRepoMock.Object,
            _serviceRepoMock.Object,
            _staffRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private Service CreateTestService(Guid branchId)
    {
        _serviceSettingsMock.Setup(s => s.SlotSizeInMinutes).Returns(15);
        _serviceSettingsMock.Setup(s => s.MinimumDurationInMinutes).Returns(15);
        _serviceSettingsMock.Setup(s => s.MaximumDurationInMinutes).Returns(240);
        _serviceSettingsMock.Setup(s => s.MaxGallerySize).Returns(10);

        var money = new Money(100m, Currency.Usd);
        var result = Service.Create(branchId, "Haircut", "desc",
            30, ServiceType.Cardiology, money, _serviceSettingsMock.Object);
        return result.Value;
    }

    private Branch CreateTestBranch(Guid orgId)
    {
        var address = new Address(123, "Main St", "City", "State", Guid.NewGuid(), 12345);
        var point = new Point(0, 0);
        var email = new Email("branch@example.com");
        var result = Branch.Create(
            orgId, address, point, email,
            new List<ServiceType> { ServiceType.Cardiology },
            FixedNow);
        return result.Value;
    }

    [TestMethod]
    public async Task Handle_ValidServices_UpdatesSuccessfully()
    {
        var service = CreateTestService(BranchId);
        var branch = CreateTestBranch(OrgId);
        var staff = Staff.Create(OrgId, FixedNow);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _branchRepoMock.Setup(r => r.GetByIdAsync(BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _staffRepoMock.Setup(r => r.GetByIdAsync(StaffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        var command = new UpdateServicesCommand(StaffId, new List<Guid> { ServiceId });
        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
    {
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var command = new UpdateServicesCommand(StaffId, new List<Guid> { ServiceId });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var service = CreateTestService(BranchId);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var branch = CreateTestBranch(OrgId);
        _branchRepoMock.Setup(r => r.GetByIdAsync(BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        _staffRepoMock.Setup(r => r.GetByIdAsync(StaffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        var command = new UpdateServicesCommand(StaffId, new List<Guid> { ServiceId });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
