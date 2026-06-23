using Viora.Application.Abstractions.Caching;

namespace Viora.Application.Organizations.GetApplicationDetails;

public sealed record GetApplicationDetailsQuery(Guid? Id = null, Guid? OwnerId = null) : ICachedQuery<ApplicationDetailsResponse>
{
    public string CacheKey => Id.HasValue
        ? $"application-{Id}"
        : $"application-owner-{OwnerId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}