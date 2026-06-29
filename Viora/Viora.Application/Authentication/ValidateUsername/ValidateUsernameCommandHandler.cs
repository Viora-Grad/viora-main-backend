using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Authentication.ValidateUsername;

internal class ValidateUsernameCommandHandler(IStaffRepository repository) : ICommandHandler<ValidateUsernameCommand>
{
    public async Task<Result> Handle(ValidateUsernameCommand request, CancellationToken cancellationToken)
    {
        var exists = await repository.GetByUsernameAsync(request.OrganizationId, request.Username, cancellationToken);

        if (exists is not null)
        {
            throw new ConflictException("Username already exists.");
        }

        return Result.Success();
    }
}
