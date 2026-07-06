using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Staffs.DeleteStaff;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class DeleteStaffCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly DeleteStaffCommandHandler _handler;

    public DeleteStaffCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new DeleteStaffCommandHandler(
            _userContextMock.Object,
            _staffRepoMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidStaff_DeletesAndSaves()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(OrgId, _fixedNow.AddDays(-30), staffId);
        var command = new DeleteStaffCommand(staffId);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(staff.IsDeleted);
        Assert.AreEqual(_fixedNow, staff.DeletedAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ReturnsSuccessIdempotent()
    {
        var command = new DeleteStaffCommand(Guid.NewGuid());

        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_StaffFromDifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(OrgId, _fixedNow.AddDays(-30), staffId);
        var command = new DeleteStaffCommand(staffId);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
