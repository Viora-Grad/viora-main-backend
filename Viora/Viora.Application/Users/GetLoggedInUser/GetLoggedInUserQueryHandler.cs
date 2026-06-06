using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Users.GetLoggedInUser;

internal class GetLoggedInUserQueryHandler(IUserContext userContext, IUserRepository userRepository) : IQueryHandler<GetLoggedInUserQuery, GetLoggedInUserResponse>
{
    public async Task<Result<GetLoggedInUserResponse>> Handle(GetLoggedInUserQuery request, CancellationToken cancellationToken)
    {
        var id = userContext.UserId;
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("User Not Found");
        var result = new GetLoggedInUserResponse
        {
            FirstName = user.PersonalInfo.FirstName,
            LastName = user.PersonalInfo.LastName,
            Email = user.Email.Value,
            DateOfBirth = user.PersonalInfo.DateOfBirth,
            Gender = user.PersonalInfo.Gender
        };
        return Result.Success(result);

    }
}
