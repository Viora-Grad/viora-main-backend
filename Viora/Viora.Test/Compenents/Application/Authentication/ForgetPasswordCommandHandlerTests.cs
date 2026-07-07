using Moq;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Authentication.ForgetPassword;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ForgetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly ForgetPasswordCommandHandler _handler;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public ForgetPasswordCommandHandlerTests()
    {
        _handler = new ForgetPasswordCommandHandler(
            _userRepoMock.Object,
            _cacheServiceMock.Object,
            _emailSenderMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidEmail_SendsOtpAndReturnsSuccess()
    {
        const string email = "user@example.com";
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var userEmail = new Viora.Domain.Users.Internal.Email(email);
        var user = User.Create(personalInfo, userEmail, FixedNow);
        var command = new ForgetPasswordCommand(email);

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cacheServiceMock.Setup(s => s.SetAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _emailSenderMock.Setup(s => s.SendAsync(
            email, It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _emailSenderMock.Verify(e => e.SendAsync(
            email, It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_EmailNotFound_ThrowsNotFoundException()
    {
        const string email = "unknown@example.com";
        var command = new ForgetPasswordCommand(email);

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
