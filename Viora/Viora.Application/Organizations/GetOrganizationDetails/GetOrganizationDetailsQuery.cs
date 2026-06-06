using Viora.Application.Abstractions.Caching;

namespace Viora.Application.Organizations.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(Guid OrganizationId) : ICachedQuery<OrganizationDetailsResponse>
{
    public string CacheKey => $"OrganizationDetails-{OrganizationId}";

    public TimeSpan? Expiration => TimeSpan.FromHours(3);
}
