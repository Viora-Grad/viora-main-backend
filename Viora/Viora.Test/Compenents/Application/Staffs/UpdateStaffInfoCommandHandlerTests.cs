using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Security;
using Viora.Application.Staffs.UpdateStaffInfo;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class UpdateStaffInfoCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly UpdateStaffInfoCommandHandler _handler;

    public UpdateStaffInfoCommandHandlerTests()
    {
        _handler = new UpdateStaffInfoCommandHandler(
            _userContextMock.Object,
            _staffRepoMock.Object,
            _hasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task Handle_UpdateAllFields_UpdatesAndSaves()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(OrgId, DateTime.UtcNow, staffId);
        staff.SetStaffProperties("Old", "Name", "old_user", "hash", new DateOnly(1980, 1, 1), Gender.Male, "+1234567890");
        var command = new UpdateStaffInfoCommand(
            staffId, "John", "Doe", "john_new", "NewPass1!",
            new DateOnly(1990, 1, 1), "Male", "+1234567890");

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Hash("NewPass1!")).Returns("new-hashed-pw");

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("John", (string)staff.FirstName!);
        Assert.AreEqual("Doe", (string)staff.LastName!);
        Assert.AreEqual("john_new", (string)staff.Username!);
        Assert.AreEqual("new-hashed-pw", (string)staff.HashedPassword!);
        Assert.AreEqual("+1234567890", (string)staff.PhoneNumber!);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var command = new UpdateStaffInfoCommand(Guid.NewGuid(), "John", null, null, null, null, null, null);

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_DifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(OrgId, DateTime.UtcNow, staffId);
        var command = new UpdateStaffInfoCommand(staffId, "John", null, null, null, null, null, null);

        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoOrgInContext_ThrowsUnauthorizedAccessException()
    {
        var command = new UpdateStaffInfoCommand(Guid.NewGuid(), "John", null, null, null, null, null, null);

        _userContextMock.Setup(c => c.OrganizationId).Returns((Guid?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
