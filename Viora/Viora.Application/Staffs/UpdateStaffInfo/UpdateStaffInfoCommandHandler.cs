using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Application.Staffs.UpdateStaffInfo;

public class UpdateStaffInfoCommandHandler(
    IUserContext userContext,
    IStaffRepository staffRepository,
    IHasher hasher,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<UpdateStaffInfoCommand>
{
    public async Task<Result> Handle(UpdateStaffInfoCommand request, CancellationToken cancellationToken)
    {
        var orgId = userContext.OrganizationId ?? throw new UnauthorizedAccessException();

        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken) ??
            throw new NotFoundException($"Staff with ID {request.StaffId} not found.");

        if (orgId != staff.OrganizationId)
            throw new UnauthorizedAccessException("You do not have permission to update this staff member.");

        FirstName firstName = request.FirstName is not null ? request.FirstName : staff.FirstName!;
        LastName lastName = request.LastName is not null ? request.LastName : staff.LastName!;
        Username username = request.Username is not null ? request.Username : staff.Username!;
        HashedPassword hashedPassword = request.Password is not null ? hasher.Hash(request.Password) : staff.HashedPassword!;
        DateOnly dateOfBirth = (DateOnly)(request.DateOfBirth is not null ? request.DateOfBirth : staff.DateOfBirth!);
        Gender gender = (Gender)(request.Gender is not null ? Enum.Parse<Gender>(request.Gender, true) : staff.Gender!);
        PhoneNumber phoneNumber = request.PhoneNumber is not null ? request.PhoneNumber : staff.PhoneNumber!;

        staff.SetStaffProperties(
            firstName,
            lastName,
            username,
            hashedPassword,
            dateOfBirth,
            gender,
            phoneNumber
            );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

}
