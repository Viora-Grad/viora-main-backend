using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Authentication.ConsumeRefreshToken;
using Viora.Domain.Abstractions;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ConsumeRefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly ConsumeRefreshTokenCommandHandler _handler;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly string Token = "refresh-token-value";

    public ConsumeRefreshTokenCommandHandlerTests()
    {
        _handler = new ConsumeRefreshTokenCommandHandler(_authServiceMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidToken_ReturnsAuthResult()
    {
        var expectedResponse = new AuthResult(
            UserId, "access-token", Token,
            new List<string> { "User" }, new List<string>());
        var command = new ConsumeRefreshTokenCommand(Token);

        _authServiceMock.Setup(s => s.RefreshTokenAsync(Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(UserId, result.Value.UserId);
        Assert.AreEqual("access-token", result.Value.AccessToken);
        Assert.AreEqual(Token, result.Value.RefreshToken);
    }

    [TestMethod]
    public async Task Handle_InvalidToken_ReturnsFailure()
    {
        const string invalidToken = "invalid-token";
        var command = new ConsumeRefreshTokenCommand(invalidToken);

        _authServiceMock.Setup(s => s.RefreshTokenAsync(invalidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthResult>(new Error("Auth.InvalidToken", "Invalid token", ErrorCategory.Validation)));

        Result<AuthResult> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
    }
}
