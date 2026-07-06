using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Staffs.GetStaffMe;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class GetStaffMeQueryHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly GetStaffMeQueryHandler _handler;

    public GetStaffMeQueryHandlerTests()
    {
        _handler = new GetStaffMeQueryHandler(_staffRepoMock.Object, _userContextMock.Object);
    }

    [TestMethod]
    public async Task Handle_StaffFound_ReturnsResponse()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow, staffId);
        var query = new GetStaffMeQuery();

        _userContextMock.Setup(c => c.UserId).Returns(staffId);
        _staffRepoMock.Setup(r => r.GetByIdWithDetailsAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        Result<StaffMeResponse> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(staffId, result.Value.Id);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ReturnsFailure()
    {
        var query = new GetStaffMeQuery();

        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _staffRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        Result<StaffMeResponse> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.StaffNotFound, result.Error);
    }
}
