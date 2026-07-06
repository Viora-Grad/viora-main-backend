using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Viora.Infrastructure;
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Repositories.Authentication;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class AuthenticationIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IIdentityRepository> _identityRepoMock = new();
    private readonly Mock<IOrganizationRepository> _orgRepoMock = new();

    private readonly ApplicationDbContext _dbContext;
    private readonly AuthenticationService _authService;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public AuthenticationIntegrationTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake-jwt-token");
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);

        _userRepoMock.Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(user => _dbContext.Add(user));
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _identityRepoMock.Setup(r => r.Add(It.IsAny<AuthIdentity>()))
            .Callback<AuthIdentity>(identity => _dbContext.Add(identity));

        var configMock = new Mock<IConfiguration>();
        var expirySectionMock = new Mock<IConfigurationSection>();
        expirySectionMock.Setup(s => s.Value).Returns("7");
        configMock.Setup(c => c.GetSection("RefreshToken:Expiry_Days")).Returns(expirySectionMock.Object);
        var secretSectionMock = new Mock<IConfigurationSection>();
        secretSectionMock.Setup(s => s.Value).Returns("test-secret-key-32-chars-long!!");
        configMock.Setup(c => c.GetSection("RefreshToken:Secret")).Returns(secretSectionMock.Object);
        var refreshTokenService = new RefreshTokenService(configMock.Object, _clockMock.Object);

        var localCredRepo = new LocalCredentialRepository(_dbContext);
        var refreshTokenRepo = new RefreshTokenRepository(_dbContext);
        var staffRefreshTokenRepo = new StaffRefreshTokenRepository(_dbContext);

        _authService = new AuthenticationService(
            _userRepoMock.Object,
            _jwtServiceMock.Object,
            _hasherMock.Object,
            _dbContext,
            _clockMock.Object,
            _identityRepoMock.Object,
            _orgRepoMock.Object,
            refreshTokenService,
            localCredRepo,
            refreshTokenRepo,
            staffRefreshTokenRepo,
            _dbContext);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task RegisterUser_WithEmailAndPassword_PersistsUserCredentialsAndIdentity()
    {
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var email = new Email("john@example.com");
        var user = User.Create(personalInfo, email, FixedNow);

        var result = await _authService.RegisterAsync(user, "StrongPass1!", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user.Id.ToString(), result.Value);

        _hasherMock.Verify(h => h.Hash("StrongPass1!"), Times.Once);

        var savedCredential = await _dbContext.Set<LocalCredential>()
            .FirstOrDefaultAsync(lc => lc.UserId == user.Id);
        Assert.IsNotNull(savedCredential);
        Assert.AreEqual("hashed-password", savedCredential.HashedPassword);
        Assert.AreEqual(0, savedCredential.FailedLoginAttempts);

        var savedIdentity = await _dbContext.Set<AuthIdentity>()
            .FirstOrDefaultAsync(ai => ai.UserId == user.Id);
        Assert.IsNotNull(savedIdentity);
        Assert.AreEqual("local", savedIdentity.Provider);
        Assert.AreEqual("john@example.com", savedIdentity.ProviderKey);

        _userRepoMock.Verify(r => r.Add(It.Is<User>(u => u.Id == user.Id)), Times.Once);
        _identityRepoMock.Verify(r => r.Add(It.Is<AuthIdentity>(i => i.UserId == user.Id)), Times.Once);
    }

    [TestMethod]
    public async Task Login_AfterRegistration_GeneratesTokenWithRoleClaims()
    {
        var personalInfo = new PersonalInfo("Jane", "Smith", new DateOnly(1992, 6, 15), Gender.Female);
        var email = new Email("jane@example.com");
        var user = User.Create(personalInfo, email, FixedNow);

        var registerResult = await _authService.RegisterAsync(user, "SecurePass1!", CancellationToken.None);
        Assert.IsTrue(registerResult.IsSuccess);

        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("SecurePass1!", "hashed-password")).Returns(true);

        var loginResult = await _authService.LocalLoginAsync("jane@example.com", "SecurePass1!", CancellationToken.None);

        Assert.IsTrue(loginResult.IsSuccess);
        Assert.AreEqual(user.Id, loginResult.Value.UserId);
        Assert.AreEqual("fake-jwt-token", loginResult.Value.AccessToken);
        Assert.IsNotNull(loginResult.Value.RefreshToken);
        Assert.IsTrue(loginResult.Value.Roles.Contains("Registered"));

        _jwtServiceMock.Verify(j => j.GenerateToken(
            user.Id,
            It.Is<IEnumerable<Claim>>(claims =>
                claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Registered"))),
            Times.AtLeastOnce);

        var savedRefreshToken = await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id && !rt.IsRevoked);
        Assert.IsNotNull(savedRefreshToken);
    }
}
