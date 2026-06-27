using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

public record GetStaffShiftByDayQuery(DateTime Date, Guid StaffId, Guid ShiftId) : IQuery<StaffDayShiftResponse>;
