using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Authentication.ChangePassword;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly ChangePasswordCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public ChangePasswordCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);
        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _handler = new ChangePasswordCommandHandler(
            _authServiceMock.Object,
            _userContextMock.Object,
            _userRepoMock.Object,
            _emailSenderMock.Object,
            _clockMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidPasswordChange_ReturnsSuccess()
    {
        const string oldPassword = "old-P@ss1";
        const string newPassword = "new-P@ss1";
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var email = new Viora.Domain.Users.Internal.Email("user@example.com");
        var user = User.Create(personalInfo, email, FixedNow);
        var command = new ChangePasswordCommand(oldPassword, newPassword, "127.0.0.1");

        _authServiceMock.Setup(s => s.ChangePassword(
                UserId, oldPassword, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _emailSenderMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AuthServiceFails_ReturnsFailure()
    {
        var command = new ChangePasswordCommand("wrong", "new", "127.0.0.1");

        _authServiceMock.Setup(s => s.ChangePassword(
                UserId, "wrong", "new", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Auth.Failed", "Password change failed", ErrorCategory.Unauthorized)));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        _emailSenderMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
