using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Application.Organizations.SearchApplications;

internal class SearchApplicationSpecification : BaseSpecification<OrganizationApplication>
{
    public SearchApplicationSpecification(SearchApplicationParameters parameters)
    {
        if (parameters.Id.HasValue)
            AddCriteria(a => a.Id == parameters.Id);

        if (parameters.OwnerId.HasValue)
            AddCriteria(a => a.OwnerId == parameters.OwnerId);

        if (parameters.ApplicationStatus.HasValue)
            AddCriteria(a => a.Status == parameters.ApplicationStatus);

        if (parameters.ReferralSource.HasValue)
            AddCriteria(a => a.ReferralSource == parameters.ReferralSource);

        ApplyOrderByDescending(a => a.SubmittedOnUtc);

        ApplyPaging((parameters.Page - 1) * parameters.PageSize, parameters.PageSize);
    }

}

internal record SearchApplicationParameters(
    Guid? Id = null,
    Guid? OwnerId = null,
    ApplicationStatus? ApplicationStatus = null,
    ReferralSource? ReferralSource = null,
    int Page = 1,
    int PageSize = 20);