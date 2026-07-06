using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Authentication.ValidateEmail;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Application.Authentication;

[TestClass]
public sealed class ValidateEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly ValidateEmailCommandHandler _handler;

    public ValidateEmailCommandHandlerTests()
    {
        _handler = new ValidateEmailCommandHandler(_userRepoMock.Object);
    }

    private static User CreateTestUser()
    {
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);
        var email = new Email("taken@example.com");
        return User.Create(personalInfo, email, DateTime.UtcNow);
    }

    [TestMethod]
    public async Task Handle_EmailAvailable_ReturnsSuccess()
    {
        const string email = "new@example.com";

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Result result = await _handler.Handle(new ValidateEmailCommand(email), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Handle_EmailTaken_ThrowsConflictException()
    {
        const string email = "taken@example.com";

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestUser());

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new ValidateEmailCommand(email), CancellationToken.None));
    }
}
