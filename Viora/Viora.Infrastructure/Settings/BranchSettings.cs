using Viora.Domain.Branches;

namespace Viora.Infrastructure.Settings;

public class BranchSettings : IBranchSettings
{
    public int MaximumGallerySize { get; set; } = default!;
}
