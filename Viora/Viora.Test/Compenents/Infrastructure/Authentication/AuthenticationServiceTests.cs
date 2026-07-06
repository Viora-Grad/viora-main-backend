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

namespace Viora.Test.Compenents.Infrastructure.Authentication;

/// <summary>
/// Tests AuthenticationService.LocalLoginAsync.
///
/// Prerequisites:
///   1. Add to Viora.Infrastructure.csproj:
///        <ItemGroup><InternalsVisibleTo Include="Viora.Test" /></ItemGroup>
///   2. Add to Viora.Application.csproj:
///        <ItemGroup><InternalsVisibleTo Include="Viora.Test" /></ItemGroup>
///   3. Add to Viora.Test.csproj:
///        <ItemGroup>
///          <ProjectReference Include="..\Viora.Application\Viora.Application.csproj" />
///          <ProjectReference Include="..\Viora.Infrastructure\Viora.Infrastructure.csproj" />
///        </ItemGroup>
///        <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.7" />
/// </summary>
[TestClass]
public sealed class AuthenticationServiceTests
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

    public AuthenticationServiceTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake-jwt-token");

        // In-memory database for concrete repositories
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);

        // Real RefreshTokenService with mocked configuration
        var configMock = new Mock<IConfiguration>();
        var expirySectionMock = new Mock<IConfigurationSection>();
        expirySectionMock.Setup(s => s.Value).Returns("7");
        configMock.Setup(c => c.GetSection("RefreshToken:Expiry_Days")).Returns(expirySectionMock.Object);
        var secretSectionMock = new Mock<IConfigurationSection>();
        secretSectionMock.Setup(s => s.Value).Returns("test-secret-key-32-chars-long!!");
        configMock.Setup(c => c.GetSection("RefreshToken:Secret")).Returns(secretSectionMock.Object);
        var refreshTokenService = new RefreshTokenService(configMock.Object, _clockMock.Object);

        // Concrete repositories backed by in-memory DbContext
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

    private static User CreateTestUser()
    {
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var email = new Email("john@example.com");
        return User.Create(personalInfo, email, FixedNow);
    }

    // ===== LocalLoginAsync =====

    [TestMethod]
    public async Task LocalLoginAsync_ValidCredentials_ReturnsAuthResult()
    {
        User user = CreateTestUser();
        var localCredential = new LocalCredential(user.Id, "hashed-password");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var localCred = new LocalCredential(user.Id, "hashed-password");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        _hasherMock.Setup(h => h.Verify("correct-password", "hashed-password")).Returns(true);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(user.Id, It.IsAny<IEnumerable<Claim>>()))
            .Returns("access-token-value");

        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "correct-password", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user.Id, result.Value.UserId);
        Assert.AreEqual("access-token-value", result.Value.AccessToken);
        Assert.IsNotNull(result.Value.RefreshToken);
        Assert.IsTrue(result.Value.Roles.Any(r => r == "Registered"));
    }

    [TestMethod]
    public async Task LocalLoginAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Result<AuthResult> result = await _service.LocalLoginAsync(
            "unknown@example.com", "any-password", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
    }

    [TestMethod]
    public async Task LocalLoginAsync_WrongPassword_ReturnsFailure()
    {
        User user = CreateTestUser();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.Verify("wrong-password", "hashed-password")).Returns(false);

        // Manually insert the local credential into the in-memory DB
        var localCred = new LocalCredential(user.Id, "hashed-password");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "wrong-password", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
    }

    [TestMethod]
    public async Task LocalLoginAsync_RecordsLoginAndCreatesRefreshToken()
    {
        User user = CreateTestUser();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.Verify("correct-password", "hashed-password")).Returns(true);

        var localCred = new LocalCredential(user.Id, "hashed-password");
        _dbContext.Set<LocalCredential>().Add(localCred);
        await _dbContext.SaveChangesAsync();

        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "correct-password", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        // Verify refresh token was persisted
        var savedTokens = await _dbContext.Set<RefreshToken>().ToListAsync();
        Assert.AreEqual(1, savedTokens.Count);
        Assert.AreEqual(user.Id, savedTokens[0].UserId);
        Assert.IsFalse(savedTokens[0].IsRevoked);
    }
}
