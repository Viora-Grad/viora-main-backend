using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Viora.Application.Abstractions.Clock;
using Viora.Infrastructure.Authentication;

namespace Viora.Test.Compenents.Infrastructure.Authentication;

[TestClass]
public sealed class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly JwtService _jwtService;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public JwtServiceTests()
    {
        _configurationMock.Setup(c => c["JWT:ISSUER"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["JWT:AUDIENCE"]).Returns("TestAudience");
        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("ThisIsASecretKeyForTesting12345678!");
        _configurationMock.Setup(c => c["JWT_EXPIRATION_MINUTES"]).Returns("60");

        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);

        _jwtService = new JwtService(_configurationMock.Object, _clockMock.Object);
    }

    [TestMethod]
    public void GenerateToken_ValidInput_ReturnsNonEmptyToken()
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, "User") };

        string token = _jwtService.GenerateToken(UserId, claims);

        Assert.IsFalse(string.IsNullOrEmpty(token));
    }

    [TestMethod]
    public void GenerateToken_DifferentUserIds_ReturnsDifferentTokens()
    {
        string token1 = _jwtService.GenerateToken(UserId, []);
        string token2 = _jwtService.GenerateToken(Guid.NewGuid(), []);

        Assert.AreNotEqual(token1, token2);
    }

    [TestMethod]
    public void GenerateToken_IncludesSubjectClaim()
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, "Admin") };

        string token = _jwtService.GenerateToken(UserId, claims);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.AreEqual(UserId.ToString(), jwtToken.Subject);
    }

    [TestMethod]
    public void GenerateToken_WithoutClaims_StillGeneratesToken()
    {
        string token = _jwtService.GenerateToken(UserId, []);

        Assert.IsFalse(string.IsNullOrEmpty(token));
        Assert.IsTrue(token.Count(c => c == '.') == 2);
    }
}
