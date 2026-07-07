using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.ChangeStatus;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class ChangeStatusCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly ChangeStatusCommandHandler _handler;

    public ChangeStatusCommandHandlerTests()
    {
        _handler = new ChangeStatusCommandHandler(
            _userContextMock.Object,
            _staffRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Staff CreateActiveStaff()
    {
        Staff staff = Staff.Create(OrgId, DateTime.UtcNow);
        staff.SetStaffProperties("John", "Doe", "john", "hash", new DateOnly(1990, 1, 1), Gender.Male, "+1234567890");
        staff.AssignBranches([CreateTestBranch()]);
        staff.Activate();
        return staff;
    }

    private static Branch CreateTestBranch()
    {
        return Branch.Create(
            Guid.NewGuid(),
            new Address(1, "St", "City", "State", Guid.NewGuid(), 12345),
            new NetTopologySuite.Geometries.Point(0, 0),
            "b@t.com",
            [ServiceType.Cardiology],
            DateTime.UtcNow).Value;
    }

    [TestMethod]
    public async Task Handle_ActivatePendingStaff_ReturnsSuccess()
    {
        Staff staff = Staff.Create(OrgId, DateTime.UtcNow);
        staff.SetStaffProperties("John", "Doe", "john", "hash", new DateOnly(1990, 1, 1), Gender.Male, "+1234567890");
        staff.AssignBranches([CreateTestBranch()]);
        var command = new ChangeStatusCommand(staff.Id, "Active");

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(StaffStatus.Active, staff.StaffStatus);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_SuspendActiveStaff_ReturnsSuccess()
    {
        Staff staff = CreateActiveStaff();
        var command = new ChangeStatusCommand(staff.Id, "Suspended");

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(StaffStatus.Suspended, staff.StaffStatus);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var command = new ChangeStatusCommand(Guid.NewGuid(), "Active");

        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_DifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Staff staff = Staff.Create(OrgId, DateTime.UtcNow);
        var command = new ChangeStatusCommand(staff.Id, "Active");

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
