using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Security;
using Viora.Application.Staffs.LoginStaff;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Test.Compenents.Application.Staffs;

[TestClass]
public sealed class LoginStaffCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IHasher> _hasherMock = new();

    private readonly LoginStaffCommandHandler _handler;

    public LoginStaffCommandHandlerTests()
    {
        _handler = new LoginStaffCommandHandler(
            _authServiceMock.Object,
            _staffRepoMock.Object,
            _hasherMock.Object);
    }

    private static Staff CreateStaffWithPassword(Guid orgId, string hashedPw)
    {
        Staff staff = Staff.Create(orgId, DateTime.UtcNow);
        typeof(Staff).GetProperty(nameof(Staff.HashedPassword))!
            .SetValue(staff, new HashedPassword(hashedPw));
        return staff;
    }

    [TestMethod]
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        Guid orgId = Guid.NewGuid();
        Staff staff = CreateStaffWithPassword(orgId, "hash123");
        var command = new LoginStaffCommand(orgId, "john", "password123");
        var authResult = new AuthResult(Guid.NewGuid(), "token", "refresh", [], []);

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(orgId, "john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify("password123", "hash123")).Returns(true);
        _authServiceMock.Setup(a => a.AuthenticateStaffAsync(staff, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResult));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(authResult, result.Value);
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsUnauthorizedAccessException()
    {
        Guid orgId = Guid.NewGuid();
        var command = new LoginStaffCommand(orgId, "unknown", "password");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(orgId, "unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        Guid orgId = Guid.NewGuid();
        Staff staff = CreateStaffWithPassword(orgId, "hash123");
        var command = new LoginStaffCommand(orgId, "john", "wrong");

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(orgId, "john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify("wrong", "hash123")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_AuthenticationServiceFails_ReturnsFailure()
    {
        Guid orgId = Guid.NewGuid();
        Staff staff = CreateStaffWithPassword(orgId, "hash123");
        var command = new LoginStaffCommand(orgId, "john", "password");
        var error = new Error("Auth.Failed", "Authentication failed", ErrorCategory.Validation);

        _staffRepoMock.Setup(r => r.GetByUsernameAsync(orgId, "john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify("password", "hash123")).Returns(true);
        _authServiceMock.Setup(a => a.AuthenticateStaffAsync(staff, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthResult>(error));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
    }
}
