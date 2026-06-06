using Viora.Domain.Shared;

namespace Viora.Infrastructure.Seeding.Data;

internal static class CountriesData
{
    public static IReadOnlyList<Country> All { get; } =
    [
        new(new Guid("a1b2c3d4-0001-0000-0000-000000000001"), "US", "United States", "American"),
        new(new Guid("a1b2c3d4-0001-0000-0000-000000000002"), "GB", "United Kingdom", "English"),
        new(new Guid("a1b2c3d4-0001-0000-0000-000000000003"), "EG", "Egypt", "Egyptian"),
        new(new Guid("a1b2c3d4-0001-0000-0000-000000000004"), "DE", "Germany", "German"),
        new(new Guid("a1b2c3d4-0001-0000-0000-000000000005"), "FR", "France", "French"),
    ];
}