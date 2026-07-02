using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.GetBranchServiceStaffs;

public sealed record GetBranchServiceStaffsQuery(Guid BranchId, Guid ServiceId) : IQuery<IEnumerable<GetBranchStaffsResponse>>;
