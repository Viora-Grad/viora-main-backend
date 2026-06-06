using Viora.Domain.Services;

namespace Viora.Infrastructure.Settings;

public class ServiceSettings : IServiceSettings
{
    public int SlotSizeInMinutes { get; set; }
    public int MinimumDurationInMinutes { get; set; }
    public int MaximumDurationInMinutes { get; set; }
    public int MaxGallerySize { get; set; }
}
