using Microsoft.Extensions.Configuration;
using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Infrastructure.Authentication;

namespace Viora.Test.Compenents.Infrastructure.Authentication;

[TestClass]
public sealed class RefreshTokenServiceTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly RefreshTokenService _service;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public RefreshTokenServiceTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);

        var expirySectionMock = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        expirySectionMock.Setup(s => s.Value).Returns("7");
        _configMock.Setup(c => c.GetSection("RefreshToken:Expiry_Days")).Returns(expirySectionMock.Object);

        var secretSectionMock = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        secretSectionMock.Setup(s => s.Value).Returns("test-secret-key-32-chars-long!!");
        _configMock.Setup(c => c.GetSection("RefreshToken:Secret")).Returns(secretSectionMock.Object);

        _service = new RefreshTokenService(_configMock.Object, _clockMock.Object);
    }

    // ===== GenerateRefreshToken =====

    [TestMethod]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        string token = _service.GenerateRefreshToken();

        Assert.IsNotNull(token);
        Assert.IsTrue(token.Length > 0);
        Assert.AreEqual(88, token.Length);
    }

    [TestMethod]
    public void GenerateRefreshToken_ConsecutiveCalls_ReturnsDifferentTokens()
    {
        string token1 = _service.GenerateRefreshToken();
        string token2 = _service.GenerateRefreshToken();

        Assert.AreNotEqual(token1, token2);
    }

    [TestMethod]
    public void GenerateRefreshToken_CanBeDecodedTo64Bytes()
    {
        string token = _service.GenerateRefreshToken();

        byte[] decoded = Convert.FromBase64String(token);

        Assert.AreEqual(64, decoded.Length);
    }

    // ===== HashToken =====

    [TestMethod]
    public void HashToken_SameToken_ReturnsSameHash()
    {
        string token = _service.GenerateRefreshToken();

        string hash1 = _service.HashToken(token);
        string hash2 = _service.HashToken(token);

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashToken_DifferentTokens_ReturnsDifferentHashes()
    {
        string token1 = _service.GenerateRefreshToken();
        string token2 = _service.GenerateRefreshToken();

        string hash1 = _service.HashToken(token1);
        string hash2 = _service.HashToken(token2);

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashToken_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.HashToken(null!));
    }

    [TestMethod]
    public void HashToken_ReturnsBase64String()
    {
        string token = _service.GenerateRefreshToken();

        string hash = _service.HashToken(token);

        Assert.IsNotNull(hash);
        Assert.AreEqual(44, hash.Length);
    }

    // ===== GetExpiryDate =====

    [TestMethod]
    public void GetExpiryDate_ReturnsDateInFuture()
    {
        DateTime expiry = _service.GetExpiryDate();

        Assert.IsTrue(expiry > FixedNow);
    }

    [TestMethod]
    public void GetExpiryDate_UsesConfiguredExpiryDays()
    {
        DateTime expiry = _service.GetExpiryDate();

        Assert.AreEqual(FixedNow.AddDays(7), expiry);
    }
}
