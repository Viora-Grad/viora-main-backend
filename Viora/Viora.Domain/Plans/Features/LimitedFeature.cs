using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Domain.Plans.Features;

public class LimitedFeature : Entity
{
    public FeatureKey Key { get; private set; } = default!;
    public FeatureDescription Description { get; private set; } = default!;
    private LimitedFeature() { } // for EF Core
    private LimitedFeature(Guid id, FeatureKey key, FeatureDescription description) : base(id)
    {
        Key = key;
        Description = description;
    }

    public static readonly LimitedFeature Branches = new(
        new Guid("f1a2b3c4-0001-0000-0000-000000000001"),
        new FeatureKey("branches"),
        new FeatureDescription("Number of branches the organization can have")
        );

    public static readonly LimitedFeature ServicesPerBranch = new(
        new Guid("f1a2b3c4-0002-0000-0000-000000000002"),
        new FeatureKey("services_per_branch"),
        new FeatureDescription("Number of services allowed per branch")
        );

    public static readonly LimitedFeature StaffMembers = new(
        new Guid("f1a2b3c4-0003-0000-0000-000000000003"),
        new FeatureKey("staff_members"),
        new FeatureDescription("Number of staff members the organization can have")
        );

    public static readonly LimitedFeature StorageBytes = new(
        new Guid("f1a2b3c4-0004-0000-0000-000000000004"),
        new FeatureKey("storage_gb"),
        new FeatureDescription("Storage quota in Bytes")
        );

    public static IReadOnlyList<LimitedFeature> All =>
        [Branches, ServicesPerBranch, StaffMembers, StorageBytes];
}
