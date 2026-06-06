using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Viora.Application.Abstractions.Caching;

namespace Viora.Infrastructure.Caching;

internal sealed class CacheService(IDistributedCache cache) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);

        await cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(key, cancellationToken);
    }
}
