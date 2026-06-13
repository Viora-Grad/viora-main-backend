using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.Shared;

internal class SearchBranchScheduleSpecification : BaseSpecification<Schedule>
{
    public SearchBranchScheduleSpecification(SearchBranchScheduleParameters parameters)
    {
        if (parameters.branchId.HasValue)
            AddCriteria(s => s.BranchId == parameters.branchId);
        if (parameters.day.HasValue)
            AddCriteria(s => s.DayOfWeek == parameters.day);
    }
}

internal record SearchBranchScheduleParameters(Guid? branchId = null, DayOfWeek? day = null);
