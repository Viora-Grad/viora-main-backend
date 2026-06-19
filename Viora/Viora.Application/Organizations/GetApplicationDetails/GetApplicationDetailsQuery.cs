using Viora.Application.Abstractions.Caching;

namespace Viora.Application.Organizations.GetApplicationDetails;

public sealed record GetApplicationDetailsQuery(Guid Id) : ICachedQuery<ApplicationDetailsResponse>
{
    public string CacheKey => $"application-{Id}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}