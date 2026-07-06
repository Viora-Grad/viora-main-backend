using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Staffs.GetStaffById;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class GetStaffByIdQueryHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly GetStaffByIdQueryHandler _handler;

    public GetStaffByIdQueryHandlerTests()
    {
        _handler = new GetStaffByIdQueryHandler(_staffRepoMock.Object);
    }

    [TestMethod]
    public async Task Handle_StaffFound_ReturnsResponse()
    {
        Guid staffId = Guid.NewGuid();
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow, staffId);
        var query = new GetStaffByIdQuery(staffId);

        _staffRepoMock.Setup(r => r.GetByIdWithDetailsAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        Result<GetStaffByIdResponse> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(staffId, result.Value.Id);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        var query = new GetStaffByIdQuery(Guid.NewGuid());

        _staffRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
