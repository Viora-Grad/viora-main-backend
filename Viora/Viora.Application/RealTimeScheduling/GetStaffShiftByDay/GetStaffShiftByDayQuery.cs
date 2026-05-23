using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDayQuery;

public record GetStaffShiftByDayQuery(DateTime time, Guid StaffId) : IQuery<List<StaffDayShiftResponse>>;
