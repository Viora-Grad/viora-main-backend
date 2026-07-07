using Moq;
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Security;
using Viora.Application.Staffs.Abstractions;
using Viora.Application.Staffs.RegisterStaff;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class RegisterStaffCommandHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IStaffTokenRepository> _tokenRepoMock = new();
    private readonly Mock<IStaffInvitationService> _invitationServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly RegisterStaffCommandHandler _handler;

    public RegisterStaffCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new RegisterStaffCommandHandler(
            _staffRepoMock.Object,
            _tokenRepoMock.Object,
            _invitationServiceMock.Object,
            _hasherMock.Object,
            _clockMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Staff CreateStaffWithRolesAndBranches(Guid orgId, DateTime createdAt, Guid? id = null)
    {
        Staff staff = Staff.Create(orgId, createdAt, id);
        staff.AddRoles([new Role(10, "Staff", null, orgId)]);
        var address = new Address(1, "St", "City", "State", Guid.NewGuid(), 12345);
        var point = new Point(0, 0);
        var branch = Branch.Create(orgId, address, point, "b@t.com", [ServiceType.Cardiology], createdAt).Value;
        staff.AssignBranches([branch]);
        return staff;
    }

    [TestMethod]
    public async Task Handle_ValidRegistration_ReturnsStaffId()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = CreateStaffWithRolesAndBranches(OrgId, _fixedNow.AddDays(-7), staffId);
        var tokenHash = "existing-hash";
        StaffToken token = StaffToken.Create(staffId, tokenHash, _fixedNow.AddDays(-7), _fixedNow.AddDays(30));

        var command = new RegisterStaffCommand(
            OrgId, "raw-token", "John", "Doe",
            new DateOnly(1990, 1, 1), "Male", "+1234567890",
            "john_doe", "StrongPass1!");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(OrgId, "john_doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-pw");
        _invitationServiceMock.Setup(s => s.HashInvitationToken("raw-token")).Returns(tokenHash);
        _tokenRepoMock.Setup(t => t.GetByTokenAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(staffId, result.Value);
        Assert.AreEqual(StaffStatus.Active, staff.StaffStatus);
        Assert.IsTrue(token.IsUsed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_UsernameTaken_ThrowsConflictException()
    {
        Staff existingStaff = Staff.Create(OrgId, _fixedNow);
        var command = new RegisterStaffCommand(
            OrgId, "token", "John", "Doe",
            new DateOnly(1990, 1, 1), "Male", "+1234567890",
            "taken_username", "Pass1!");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(OrgId, "taken_username", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStaff);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_InvalidToken_ReturnsFailure()
    {
        var tokenHash = "expired-hash";
        var expiredToken = StaffToken.Create(Guid.NewGuid(), tokenHash, _fixedNow.AddDays(-30), _fixedNow.AddDays(-1));

        var command = new RegisterStaffCommand(
            OrgId, "bad-token", "John", "Doe",
            new DateOnly(1990, 1, 1), "Male", "+1234567890",
            "new_user", "Pass1!");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(OrgId, "new_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);
        _invitationServiceMock.Setup(s => s.HashInvitationToken("bad-token")).Returns(tokenHash);
        _tokenRepoMock.Setup(t => t.GetByTokenAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var staff = Staff.Create(OrgId, _fixedNow.AddDays(-7));
        _staffRepoMock.Setup(r => r.GetByIdAsync(expiredToken.StaffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.InvalidInvitationToken, result.Error);
    }

    [TestMethod]
    public async Task Handle_TokenNotFound_ThrowsNotFoundException()
    {
        var command = new RegisterStaffCommand(
            OrgId, "unknown-token", "John", "Doe",
            new DateOnly(1990, 1, 1), "Male", "+1234567890",
            "new_user", "Pass1!");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(OrgId, "new_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);
        _invitationServiceMock.Setup(s => s.HashInvitationToken("unknown-token")).Returns("hash");
        _tokenRepoMock.Setup(t => t.GetByTokenAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StaffToken?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
