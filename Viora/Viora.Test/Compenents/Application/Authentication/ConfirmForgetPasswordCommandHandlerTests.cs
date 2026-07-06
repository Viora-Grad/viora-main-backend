using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Authentication.ConfirmForgetPassword;
using Viora.Domain.Abstractions;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ConfirmForgetPasswordCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly ConfirmForgetPasswordCommandHandler _handler;

    private readonly DateTime _fixedNow = new(2026, 7, 6, 14, 0, 0, DateTimeKind.Utc);

    public ConfirmForgetPasswordCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new ConfirmForgetPasswordCommandHandler(
            _authServiceMock.Object,
            _cacheServiceMock.Object,
            _emailSenderMock.Object,
            _clockMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidOtp_UpdatesPasswordAndReturnsSuccess()
    {
        const string email = "user@example.com";
        const string otp = "123456";
        const string newPassword = "New-P@ss1";
        const string cacheKey = $"forget-password-{email}";
        var command = new ConfirmForgetPasswordCommand(email, otp, newPassword, "127.0.0.1");

        _cacheServiceMock.Setup(c => c.GetAsync<string>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otp);
        _authServiceMock.Setup(s => s.UpdatePassword(email, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _emailSenderMock.Setup(s => s.SendAsync(
            email, It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _authServiceMock.Verify(s => s.UpdatePassword(email, newPassword, It.IsAny<CancellationToken>()), Times.Once);
        _emailSenderMock.Verify(e => e.SendAsync(
            email, It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_InvalidOtp_ThrowsConflictException()
    {
        const string email = "user@example.com";
        const string cachedOtp = "654321";
        const string wrongOtp = "123456";
        var command = new ConfirmForgetPasswordCommand(email, wrongOtp, "New-P@ss1", "127.0.0.1");

        _cacheServiceMock.Setup(c => c.GetAsync<string>($"forget-password-{email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedOtp);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _authServiceMock.Verify(s => s.UpdatePassword(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_ExpiredOtp_ThrowsNotFoundException()
    {
        const string email = "user@example.com";
        var command = new ConfirmForgetPasswordCommand(email, "123456", "New-P@ss1", "127.0.0.1");

        _cacheServiceMock.Setup(c => c.GetAsync<string>($"forget-password-{email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _authServiceMock.Verify(s => s.UpdatePassword(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
