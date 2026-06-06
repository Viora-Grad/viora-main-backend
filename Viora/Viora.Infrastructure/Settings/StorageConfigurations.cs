using Viora.Application.Abstractions.Media;

namespace Viora.Infrastructure.Settings;

public class StorageConfigurations : IStorageSettings
{
    public string BasePath { get; set; } = null!;

    public long MaxFileSizeBytes { get; set; }
}
