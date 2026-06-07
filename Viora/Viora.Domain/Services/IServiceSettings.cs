namespace Viora.Domain.Services;

public interface IServiceSettings
{
    int SlotSizeInMinutes { get; set; }
    int MinimumDurationInMinutes { get; set; }
    int MaximumDurationInMinutes { get; set; }
    int MaxGallerySize { get; set; }
}
