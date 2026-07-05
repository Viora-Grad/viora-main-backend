using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.SearchStaff;

internal class SearchStaffQueryHandler(
    IStaffRepository staffRepository
    ) : IQueryHandler<SearchStaffQuery, PaginatedModel<SearchStaffResponse>>
{
    public async Task<Result<PaginatedModel<SearchStaffResponse>>> Handle(SearchStaffQuery request, CancellationToken cancellationToken)
    {
        var parameters = new SearchStaffParameters(
            StaffId: request.StaffId,
            OrganizationId: request.OrganizationId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Gender: request.Gender,
            RoleIds: request.RoleIds,
            BranchIds: request.BranchIds,
            ServiceIds: request.ServiceIds,
            Statuses: request.Statuses,
            Page: request.Page,
            PageSize: request.PageSize
        );
        var specs = new SearchStaffSpecification(parameters);

        var Staffs = await staffRepository.ListAsync(specs, cancellationToken);
        var response = Staffs.Select(staff => new SearchStaffResponse
        {
            Id = staff.Id,
            FirstName = staff.FirstName?.Value,
            LastName = staff.LastName?.Value,
            PhoneNumber = staff.PhoneNumber,
            Gender = staff.Gender.ToString(),
            DateOfBirth = staff.DateOfBirth,
            status = staff.StaffStatus.ToString()
        }
        ).ToList();

        return Result.Success(new PaginatedModel<SearchStaffResponse>(response, request.Page, request.PageSize, Staffs.Count));
    }
}
