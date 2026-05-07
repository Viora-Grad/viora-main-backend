using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Organizations.SearchApplications;

internal class SearchApplicationsQueryHandler(
    IOrganizationApplicationRepository applicationRepository,
    IUserRepository userRepository) : IQueryHandler<SearchApplicationsQuery, PaginatedModel<ApplicationsResponse>>
{
    public async Task<Result<PaginatedModel<ApplicationsResponse>>> Handle(SearchApplicationsQuery request, CancellationToken cancellationToken)
    {
        ApplicationStatus? status = request.Status != null ? Enum.Parse<ApplicationStatus>(request.Status, ignoreCase: true) : null;
        ReferralSource? referralSource = request.ReferralSource != null ? Enum.Parse<ReferralSource>(request.ReferralSource, ignoreCase: true) : null;

        var specificationParams = new SearchApplicationParameters(request.Id, request.OwnerId, status, referralSource, request.Page, request.PageSize);

        var speicifcation = new SearchApplicationSpecification(specificationParams);

        var repoResult = await applicationRepository.ListAsync(speicifcation, cancellationToken);
        var countResults = await applicationRepository.CountAsync(speicifcation, cancellationToken);

        var usersDict = await userRepository.GetNamesDictAsync(repoResult.Select(r => r.OwnerId), cancellationToken);

        var applicationResponseModel = repoResult.Select(a =>
        {
            return new ApplicationsResponse(a.Id,
                a.OwnerId,
                usersDict[a.OwnerId],
                a.ProposedName,
                a.ApplicationLetter,
                a.ServiceDescription,
                a.ProposedServiceType.ToString(),
                a.SubmittedOnUtc,
                a.Status.ToString(),
                a.ReferralSource.ToString(),
                a.RejectedBy,
                a.RejectedBy != null ? usersDict[(Guid)a.RejectedBy] : null,
                a.ExpiryDateUtc,
                a.BillingEmail,
                a.SupportEmail);
        });

        PaginatedModel<ApplicationsResponse> paginatedResponse = new(applicationResponseModel, request.Page, request.PageSize, countResults);

        return Result.Success(paginatedResponse);
    }
}