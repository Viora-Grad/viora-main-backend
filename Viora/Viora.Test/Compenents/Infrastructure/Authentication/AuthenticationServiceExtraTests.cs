using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Viora.Infrastructure;
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Repositories.Authentication;

namespace Viora.Test.Compenents.Infrastructure.Authentication;

[TestClass]
public sealed class AuthenticationServiceExtraTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IIdentityRepository> _identityRepoMock = new();
    private readonly Mock<IOrganizationRepository> _orgRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    private readonly ApplicationDbContext _dbContext;
    private readonly AuthenticationService _service;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public AuthenticationServiceExtraTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake-jwt-token");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);

        // Delegate mocked Add calls to the real in-memory DbContext
        _userRepoMock.Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => _dbContext.Set<User>().Add(u));

        var refreshTokenService = CreateRefreshTokenService(_clockMock.Object);

        var localCredRepo = new LocalCredentialRepository(_dbContext);
        var refreshTokenRepo = new RefreshTokenRepository(_dbContext);
        var staffRefreshTokenRepo = new StaffRefreshTokenRepository(_dbContext);

        _service = new AuthenticationService(
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

    private static RefreshTokenService CreateRefreshTokenService(IDateTimeProvider clock)
    {
        var configMock = new Mock<IConfiguration>();
        var expirySectionMock = new Mock<IConfigurationSection>();
        expirySectionMock.Setup(s => s.Value).Returns("7");
        configMock.Setup(c => c.GetSection("RefreshToken:Expiry_Days")).Returns(expirySectionMock.Object);
        var secretSectionMock = new Mock<IConfigurationSection>();
        secretSectionMock.Setup(s => s.Value).Returns("test-secret-key-32-chars-long!!");
        configMock.Setup(c => c.GetSection("RefreshToken:Secret")).Returns(secretSectionMock.Object);
        return new RefreshTokenService(configMock.Object, clock);
    }

    private static User CreateTestUser()
    {
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var email = new Email("john@example.com");
        return User.Create(personalInfo, email, FixedNow);
    }

    // ===== RegisterAsync =====

    [TestMethod]
    public async Task RegisterAsync_NewUser_ReturnsUserId()
    {
        User user = CreateTestUser();
        const string password = "SecureP@ss1";

        _hasherMock.Setup(h => h.Hash(password)).Returns("hashed-password");

        Result<string> result = await _service.RegisterAsync(user, password, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user.Id.ToString(), result.Value);

        var savedUser = await _dbContext.Set<User>().FindAsync([user.Id], CancellationToken.None);
        Assert.IsNotNull(savedUser);
    }

    [TestMethod]
    public async Task RegisterAsync_ExistingEmail_ReturnsFailure()
    {
        User user = CreateTestUser();

        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Result<string> result = await _service.RegisterAsync(user, "pwd", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.EmailInUse, result.Error);
    }

    // ===== RefreshTokenAsync =====

    [TestMethod]
    public async Task RefreshTokenAsync_ValidToken_ReturnsAuthResult()
    {
        User user = CreateTestUser();

        var refreshTokenService = CreateRefreshTokenService(_clockMock.Object);
        var rawToken = refreshTokenService.GenerateRefreshToken();
        var hashedToken = refreshTokenService.HashToken(rawToken);
        var expiry = refreshTokenService.GetExpiryDate();
        var refreshToken = RefreshToken.Create(user.Id, hashedToken, expiry, FixedNow);

        _dbContext.Set<RefreshToken>().Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Result<AuthResult> result = await _service.RefreshTokenAsync(rawToken, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user.Id, result.Value.UserId);
        Assert.IsNull(result.Value.RefreshToken);
        Assert.AreEqual("fake-jwt-token", result.Value.AccessToken);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_ExpiredToken_ReturnsFailure()
    {
        var refreshTokenService = CreateRefreshTokenService(_clockMock.Object);
        var rawToken = refreshTokenService.GenerateRefreshToken();
        var hashedToken = refreshTokenService.HashToken(rawToken);
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), hashedToken, FixedNow.AddDays(-1), FixedNow.AddDays(-8));

        _dbContext.Set<RefreshToken>().Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        Result<AuthResult> result = await _service.RefreshTokenAsync(rawToken, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AuthenticationErrors.InvalidToken, result.Error);
    }

    // ===== LogoutAsync =====

    [TestMethod]
    public async Task LogoutAsync_ValidToken_RevokesAndReturnsSuccess()
    {
        var refreshTokenService = CreateRefreshTokenService(_clockMock.Object);
        var rawToken = refreshTokenService.GenerateRefreshToken();
        var hashedToken = refreshTokenService.HashToken(rawToken);
        var expiry = refreshTokenService.GetExpiryDate();
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), hashedToken, expiry, FixedNow);

        _dbContext.Set<RefreshToken>().Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        Result result = await _service.LogoutAsync(rawToken, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(refreshToken.IsRevoked);
    }

    [TestMethod]
    public async Task LogoutAsync_InvalidToken_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.LogoutAsync("nonexistent-token", CancellationToken.None));
    }

    // ===== UpdatePassword =====

    [TestMethod]
    public async Task UpdatePassword_ValidUser_ReturnsSuccess()
    {
        User user = CreateTestUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var localCred = new LocalCredential(user.Id, "old-hash");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        _hasherMock.Setup(h => h.Hash("new-password")).Returns("new-hash");

        Result result = await _service.UpdatePassword(user.Email.Value, "new-password", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("new-hash", localCred.HashedPassword);
    }

    [TestMethod]
    public async Task UpdatePassword_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Result result = await _service.UpdatePassword("unknown@example.com", "new-pwd", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.NotFound, result.Error);
    }

    // ===== ChangePassword =====

    [TestMethod]
    public async Task ChangePassword_ValidCredentials_ReturnsSuccess()
    {
        var localCred = new LocalCredential(UserId, "current-hash");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        _hasherMock.Setup(h => h.Verify("current-pwd", "current-hash")).Returns(true);
        _hasherMock.Setup(h => h.Hash("new-pwd")).Returns("new-hash");

        Result result = await _service.ChangePassword(UserId, "current-pwd", "new-pwd", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("new-hash", localCred.HashedPassword);
    }

    [TestMethod]
    public async Task ChangePassword_WrongPassword_ReturnsFailure()
    {
        var localCred = new LocalCredential(UserId, "current-hash");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        _hasherMock.Setup(h => h.Verify("wrong-pwd", "current-hash")).Returns(false);

        Result result = await _service.ChangePassword(UserId, "wrong-pwd", "new-pwd", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
    }
}
