using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.LoginStaff;

internal class LoginStaffCommandHandler(
    IAuthenticationService authenticationService,
    IStaffRepository staffRepository,
    IHasher hasher
    ) : ICommandHandler<LoginStaffCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(LoginStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByUsernameAsync(request.OrganizationId, request.Username, cancellationToken) ??
            throw new UnauthorizedAccessException("Invalid username or password.");

        if (!hasher.Verify(request.Password, staff.HashedPassword!))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var authResult = await authenticationService.AuthenticateStaffAsync(staff, cancellationToken);

        if (authResult.IsFailure)
            return Result.Failure<AuthResult>(authResult.Error);

        return Result.Success(authResult.Value);
    }
}
