using Viora.Application.Abstraction.Messaging;

namespace Viora.Application.Abstraction.Cashing;

public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery;

public interface ICachedQuery
{
    string CacheKey { get; }

    TimeSpan? Expiration { get; }
}