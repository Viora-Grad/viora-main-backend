using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftQuery;

public record GetStaffShiftQuery(Guid StaffId, Guid BranchId) : IQuery<List<StaffShiftResponse>>;
