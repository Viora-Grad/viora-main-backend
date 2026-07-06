using Moq;
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.Abstractions;
using Viora.Application.Staffs.CreateStaffInvitation;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class CreateStaffInvitationCommandHandlerTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IOrganizationRepository> _orgRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IStaffTokenRepository> _staffTokenRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IStaffInvitationService> _staffServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateStaffInvitationCommandHandler _handler;

    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid CountryId = Guid.NewGuid();
    private static readonly Role StaffRole = new(10, "Staff", null, OrgId);
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private const string InvitationToken = "invitation-token-value";

    public CreateStaffInvitationCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);
        _handler = new CreateStaffInvitationCommandHandler(
            _clockMock.Object,
            _orgRepoMock.Object,
            _staffRepoMock.Object,
            _staffTokenRepoMock.Object,
            _branchRepoMock.Object,
            _roleRepoMock.Object,
            _staffServiceMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object);
    }

    private Organization CreateTestOrganization()
    {
        var result = Organization.Create(
            OwnerId, CountryId, "Test Org", "About us",
            "Services", new List<ServiceType> { ServiceType.Cardiology },
            FixedNow, ReferralSource.SocialMedia,
            "billing@example.com", "support@example.com");
        return result.Value;
    }

    private Branch CreateTestBranch()
    {
        var address = new Address(123, "Main St", "City", "State", Guid.NewGuid(), 12345);
        var point = new Point(0, 0);
        var email = new Email("branch@example.com");
        var result = Branch.Create(
            OrgId, address, point, email,
            new List<ServiceType> { ServiceType.Cardiology },
            FixedNow);
        return result.Value;
    }

    [TestMethod]
    public async Task Handle_OwnerCreatesInvitation_ReturnsToken()
    {
        var organization = CreateTestOrganization();
        var branch = CreateTestBranch();
        var branchId = branch.Id;

        _orgRepoMock.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        _userContextMock.Setup(c => c.UserId).Returns(OwnerId);
        _roleRepoMock.Setup(r => r.GetOrganizationRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { StaffRole });
        _branchRepoMock.Setup(r => r.GetByOrganizationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { branch });
        _staffServiceMock.Setup(s => s.GenerateInvitationToken()).Returns(InvitationToken);
        _staffServiceMock.Setup(s => s.HashInvitationToken(InvitationToken)).Returns("hashed-token");
        _staffServiceMock.Setup(s => s.GetExpiryDate()).Returns(FixedNow.AddDays(7));

        var command = new CreateStaffInvitationCommand(
            OrgId, new List<int> { StaffRole.Id }, new List<Guid> { branchId });
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(InvitationToken, result.Value);
        _staffTokenRepoMock.Verify(r => r.Add(It.IsAny<StaffToken>()), Times.Once);
        _staffRepoMock.Verify(r => r.Add(It.IsAny<Staff>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        _orgRepoMock.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var command = new CreateStaffInvitationCommand(
            OrgId, new List<int> { StaffRole.Id }, new List<Guid> { BranchId });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NotOwnerNotStaff_ThrowsUnauthorizedAccessException()
    {
        var organization = CreateTestOrganization();

        _orgRepoMock.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _userContextMock.Setup(c => c.UserType).Returns("customer");

        var command = new CreateStaffInvitationCommand(
            OrgId, new List<int> { StaffRole.Id }, new List<Guid> { BranchId });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
