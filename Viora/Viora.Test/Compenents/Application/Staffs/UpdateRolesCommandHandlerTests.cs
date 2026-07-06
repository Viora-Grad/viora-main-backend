using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.UpdateRoles;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class UpdateRolesCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly UpdateRolesCommandHandler _handler;

    public UpdateRolesCommandHandlerTests()
    {
        _handler = new UpdateRolesCommandHandler(
            _roleRepoMock.Object,
            _staffRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidRoles_ReplacesAndSaves()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow, staffId);
        staff.AddRoles([new Role(1, "OldRole")]);
        var command = new UpdateRolesCommand(staffId, [2, 3]);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _roleRepoMock.Setup(r => r.GetOrganizationRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Role(2, "Doctor"), new Role(3, "Nurse")]);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, staff.Roles.Count);
        Assert.IsFalse(staff.Roles.Any(r => r.Id == 1));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var command = new UpdateRolesCommand(Guid.NewGuid(), [1]);

        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
