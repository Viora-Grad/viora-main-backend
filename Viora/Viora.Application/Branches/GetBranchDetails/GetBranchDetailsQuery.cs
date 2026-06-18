using Viora.Application.Abstractions.Caching;

namespace Viora.Application.Branches.GetBranchDetails;

public record GetBranchDetailsQuery(Guid Id) : ICachedQuery<BranchDetailsResponse>
{
    public string CacheKey => $"BranchDetails-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}