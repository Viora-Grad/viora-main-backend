using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Application.Staffs.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Application.Staffs.RegisterStaff;

internal class RegisterStaffCommandHandler(
    IStaffRepository staffRepository,
    IStaffTokenRepository tokenRepository,
    IStaffInvitationService invitationService,
    //ILimitedFeatureUsageService usageService,
    IHasher hasher,
    IDateTimeProvider timeProvider,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<RegisterStaffCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterStaffCommand request, CancellationToken cancellationToken)
    {
        var Exists = await staffRepository.GetByUsernameAsync(request.OrganizationId, request.Username, cancellationToken);
        if (Exists is not null) throw new ConflictException("Username already in use");

        var HashedToken = invitationService.HashInvitationToken(request.Token);

        var token = await tokenRepository.GetByTokenAsync(HashedToken, cancellationToken) ??
            throw new NotFoundException("invitation token not found");

        if (!token.IsValid(timeProvider.UtcNow))
            return Result.Failure<Guid>(StaffErrors.InvalidInvitationToken);

        var staff = token.Staff;

        if (staff is null)
            return Result.Failure<Guid>(StaffErrors.StaffNotFound);

        if (staff.OrganizationId != request.OrganizationId)
            return Result.Failure<Guid>(StaffErrors.InvalidInvitationToken);


        FirstName firstName = request.FirstName;
        LastName lastName = request.LastName;
        HashedPassword hashedPassword = hasher.Hash(request.Password);
        Username username = request.Username;
        DateOnly dateOfBirth = request.DateOfBirth;
        Gender gender = Enum.Parse<Gender>(request.Gender, true);
        PhoneNumber phoneNumber = request.PhoneNumber;
        /*
         * commented out cuz Feature usage is not seeded with existing organizations, so it will always fail for now
        var consumable = await usageService.CheckLimitAsync(staff.OrganizationId, LimitedFeature.StaffMembers.Id, -1, cancellationToken);
        if (consumable.IsFailure)
            return Result.Failure<Guid>(consumable.Error);

        var consume = await usageService.ConsumeLimit(staff.OrganizationId, LimitedFeature.StaffMembers.Id, -1, cancellationToken);
        */
        staff.SetStaffProperties(firstName, lastName, username, hashedPassword, dateOfBirth, gender, phoneNumber);
        staff.Activate();

        token.MarkAsUsed(timeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(staff.Id);
    }
}
