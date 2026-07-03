using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Staffs.GetStaffMe;

internal sealed class GetStaffMeQueryHandler(
    IStaffRepository staffRepository,
    IUserContext userContext) : IQueryHandler<GetStaffMeQuery, StaffMeResponse>
{
    public async Task<Result<StaffMeResponse>> Handle(GetStaffMeQuery request, CancellationToken cancellationToken)
    {
        // For a staff token the subject (UserContext.UserId) is the staff id.
        var staffId = userContext.UserId;

        var staff = await staffRepository.GetByIdWithDetailsAsync(staffId, cancellationToken);
        if (staff is null)
            return Result.Failure<StaffMeResponse>(StaffErrors.StaffNotFound);

        var response = new StaffMeResponse(
            staff.Id,
            staff.OrganizationId,
            staff.FirstName?.Value,
            staff.LastName?.Value,
            staff.Username?.Value,
            staff.PhoneNumber?.Value,
            staff.Gender?.ToString(),
            staff.DateOfBirth,
            staff.StaffStatus.ToString(),
            staff.CreatedAt,
            staff.Roles.SelectMany(role => role.Permissions).Select(permission => permission.Name).Distinct().ToList(),
            staff.Roles.Select(MapRole).ToList(),
            staff.Branches.Select(MapBranch).ToList(),
            staff.Services.Select(MapService).ToList());

        return Result.Success(response);
    }

    private static StaffRoleResponse MapRole(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.Permissions.Select(p => new StaffPermissionResponse(p.Id, p.Name, p.Description)).ToList());

    private static StaffBranchResponse MapBranch(Branch branch) => new(
        branch.Id,
        branch.OrganizationId,
        branch.Status.ToString(),
        new StaffBranchAddressResponse(
            branch.Address.Number,
            branch.Address.Street,
            branch.Address.City,
            branch.Address.State,
            branch.Address.CountryId,
            branch.Address.PostalCode),
        branch.ContactEmail.Value,
        branch.TimeZone.Value,
        branch.Location.Y, // NetTopologySuite Point: Y = latitude
        branch.Location.X, // X = longitude
        branch.OpenedAtUtc,
        branch.ServicesProvided.Select(serviceType => serviceType.Value).ToList(),
        branch.PhoneNumbers.Select(phone => phone.Value).ToList(),
        branch.BusinessHours.Select(bh => new StaffBusinessHourResponse(bh.Day.ToString(), bh.OpenTime, bh.CloseTime)).ToList());

    private static StaffServiceResponse MapService(Service service) => new(
        service.Id,
        service.BranchId,
        service.Name.Value,
        service.Description.Value,
        service.Type.Value,
        service.Status.ToString(),
        (int)service.Duration.TotalMinutes,
        service.Cost.Amount,
        service.Cost.Currency.Code,
        service.Discount is null
            ? null
            : new StaffServiceDiscountResponse(
                service.Discount.PercentageOutOf100,
                service.Discount.Reason,
                service.Discount.StartDateUtc,
                service.Discount.EndDateUtc));
}
