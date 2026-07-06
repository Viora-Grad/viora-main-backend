using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.AssignRoles;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class AssignRolesCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly AssignRolesCommandHandler _handler;

    public AssignRolesCommandHandlerTests()
    {
        _handler = new AssignRolesCommandHandler(
            _roleRepoMock.Object,
            _staffRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidRoles_AssignsAndSaves()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow, staffId);
        var roles = new List<Role> { new(1, "Doctor"), new(2, "Nurse") };
        var command = new AssignRolesCommand(staffId, [1, 2]);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _roleRepoMock.Setup(r => r.GetOrganizationRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, staff.Roles.Count);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var command = new AssignRolesCommand(Guid.NewGuid(), [1]);

        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_RoleIdMismatch_ThrowsNotFoundException()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        var command = new AssignRolesCommand(staff.Id, [1, 99]);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _roleRepoMock.Setup(r => r.GetOrganizationRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
