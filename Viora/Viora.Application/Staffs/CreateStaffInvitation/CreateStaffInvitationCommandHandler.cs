using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Staffs.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Staffs.CreateStaffInvitation;

internal class CreateStaffInvitationCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IOrganizationRepository organizationRepository,
    IStaffRepository staffRepository,
    IStaffTokenRepository staffTokenRepository,
    IBranchRepository branchRepository,
    // ILimitedFeatureUsageService limitedFeatureUsageService,
    IRoleRepository roleRepository,
    IStaffInvitationService staffService,
    IUserContext userContext,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateStaffInvitationCommand, string>
{
    public async Task<Result<string>> Handle(CreateStaffInvitationCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken) ??
            throw new NotFoundException($"Organization with ID {request.OrganizationId} not found");

        var userId = userContext.UserId;
        var IsOwner = userId == organization.OwnerId;
        if (!IsOwner)
        {
            var IsStaff = userContext.UserType == "staff";
            if (!IsStaff)
            {
                throw new UnauthorizedAccessException("User is not authorized to create staff invitations.");
            }
        }
        /* 
         * commented out cuz Feature Usage Limitation is not seeded with existing organizations, so it will always return false for existing organizations
        var allowed = await limitedFeatureUsageService.CheckLimitAsync(
            organization.Id,
            LimitedFeature.StaffMembers.Id,
            -1,
            cancellationToken);
        if (allowed.IsFailure)
        {
            return Result.Failure<string>(allowed.Error);
        }*/
        var orgRoles = await roleRepository.GetOrganizationRolesAsync(organization.Id, cancellationToken);
        var roles = orgRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();

        if (roles.Count != request.RoleIds.Count)
        {
            throw new NotFoundException("One or more requested roles not found.");
        }

        var branches = await branchRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);
        var selectedBranches = branches.Where(b => request.BranchIds.Contains(b.Id)).ToList();



        if (selectedBranches.Count != request.BranchIds.Count)
        {
            throw new NotFoundException("One or more requested branches not found.");
        }

        foreach (var branch in selectedBranches)
            branchRepository.Attach(branch);

        var staff = Staff.Create(
            organization.Id,
            dateTimeProvider.UtcNow);

        var token = staffService.GenerateInvitationToken();
        var tokenHash = staffService.HashInvitationToken(token);
        var expiry = staffService.GetExpiryDate();
        var staffToken = StaffToken.Create(
            staff.Id,
            tokenHash,
            dateTimeProvider.UtcNow,
            expiry);

        staff.AddRoles(roles);
        staff.AssignBranches(selectedBranches);

        staffTokenRepository.Add(staffToken);
        staffRepository.Add(staff);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(token);

    }

}








