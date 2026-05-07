using Viora.Application.Abstractions.Media;

namespace Viora.Infrastructure.Settings;

public class StorageConfigurations : IStorageConfiguration
{
    public string BasePath { get; set; } = null!;
}
