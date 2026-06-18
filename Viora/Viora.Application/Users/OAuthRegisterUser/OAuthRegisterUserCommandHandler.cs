using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.OAuthRegisterUser;

internal class OAuthRegisterUserCommandHandler(
    IAuthenticationService authenticationService,
    IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<OAuthRegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(OAuthRegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
            new PersonalInfo(request.FirstName, request.LastName, request.DateOfBirth, Enum.Parse<Gender>(request.Gender)),
            new Email(request.Email),
            dateTimeProvider.UtcNow
            );
        var identity = AuthIdentity.Create(request.Provider, user.Id, request.ProviderKey, dateTimeProvider.UtcNow);
        var linkResult = user.LinkIdentity(identity);
        if (linkResult.IsFailure)
            return Result.Failure<Guid>(linkResult.Error);

        var socialRegisterResult = await authenticationService.SocialRegisterAsync(user, identity, cancellationToken);
        if (socialRegisterResult.IsFailure)
            return Result.Failure<Guid>(socialRegisterResult.Error);

        return Result.Success(user.Id);


    }
}
