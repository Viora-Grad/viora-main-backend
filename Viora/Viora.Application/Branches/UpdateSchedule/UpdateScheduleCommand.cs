using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.UpdateSchedule;

public sealed record UpdateScheduleCommand(Guid BranchId, IEnumerable<BusinessHour> Schedule) : ICommand;
