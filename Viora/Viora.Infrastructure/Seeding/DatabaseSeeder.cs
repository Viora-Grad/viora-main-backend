using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Viora.Domain.Shared;
using Viora.Infrastructure.Seeding.Data;

namespace Viora.Infrastructure.Seeding;

public interface IDatabaseSeeder
{
    public Task SeedAsync(CancellationToken cancellationToken = default);
}
internal class DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        #region Country
        var existingIds = await db.Set<Country>()
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missing = CountriesData.All
            .Where(c => !existingIds.Contains(c.Id))
            .ToList();

        if (missing.Count == 0)
        {
            logger.LogInformation("Country seed: all {Count} countries already present.",
                CountriesData.All.Count);
            return;
        }

        await db.Set<Country>().AddRangeAsync(missing, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Country seed: inserted {Count} new countries.", missing.Count);
        #endregion Country
    }
}
