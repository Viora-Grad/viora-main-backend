using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.GetBranchServiceStaffs;

internal class GetBranchServiceStaffsQueryHandler(
    IStaffRepository staffRepository
    ) : IQueryHandler<GetBranchServiceStaffsQuery, IEnumerable<GetBranchStaffsResponse>>
{
    public async Task<Result<IEnumerable<GetBranchStaffsResponse>>> Handle(GetBranchServiceStaffsQuery request, CancellationToken cancellationToken)
    {
        var staffs = await staffRepository.GetBranchServiceStaffsAsync(request.BranchId, request.ServiceId, cancellationToken);
        if (staffs == null)
        {
            return Result.Success(Enumerable.Empty<GetBranchStaffsResponse>());
        }
        var response = staffs.Select(staff => new GetBranchStaffsResponse
        {
            Id = staff.Id,
            FirstName = staff.FirstName!,
            LastName = staff.LastName!,
            PhoneNumber = staff.PhoneNumber!,
            Gender = staff.Gender.ToString()!,
            DateOfBirth = (DateOnly)staff.DateOfBirth!
        });

        return Result.Success(response);
    }
}
