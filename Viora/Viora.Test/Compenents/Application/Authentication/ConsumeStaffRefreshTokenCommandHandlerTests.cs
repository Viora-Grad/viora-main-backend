using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Authentication.ConsumeStaffRefreshToken;
using Viora.Domain.Abstractions;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ConsumeStaffRefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly ConsumeStaffRefreshTokenCommandHandler _handler;
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly string Token = "staff-refresh-token";

    public ConsumeStaffRefreshTokenCommandHandlerTests()
    {
        _handler = new ConsumeStaffRefreshTokenCommandHandler(_authServiceMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidToken_ReturnsAuthResult()
    {
        var expectedResponse = new AuthResult(
            StaffId, "staff-access-token", Token,
            new List<string> { "Staff" }, new List<string>());
        var command = new ConsumeStaffRefreshTokenCommand(Token);

        _authServiceMock.Setup(s => s.RefreshStaffTokenAsync(Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(StaffId, result.Value.UserId);
        Assert.AreEqual("staff-access-token", result.Value.AccessToken);
        Assert.AreEqual(Token, result.Value.RefreshToken);
    }

    [TestMethod]
    public async Task Handle_InvalidToken_ReturnsFailure()
    {
        const string invalidToken = "invalid-staff-token";
        var command = new ConsumeStaffRefreshTokenCommand(invalidToken);

        _authServiceMock.Setup(s => s.RefreshStaffTokenAsync(invalidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthResult>(new Error("Auth.InvalidToken", "Invalid staff token", ErrorCategory.Validation)));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
    }
}
