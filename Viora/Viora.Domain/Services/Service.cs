using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Services.Internals;
using Viora.Domain.Shared;

namespace Viora.Domain.Services;

/// <summary>
/// defines the service provided by branch and the related category this service is of type 
/// </summary>
public sealed class Service : Entity
{
    public Guid BranchId { get; private set; }
    public ServiceName Name { get; private set; } = default!;
    public ServiceDescription Description { get; private set; } = default!;
    public ServiceType Type { get; private set; }
    public ServiceStatus Status { get; private set; }

    public TimeSpan Duration { get; private set; }
    public Money Cost { get; private set; } = default!;

    // TODO for better UX add description per image for what this image represents, not prio now, done later (dunno when xd)
    private readonly List<MediaFile> _gallery = [];
    public IReadOnlyCollection<MediaFile> Gallery => _gallery.AsReadOnly();

    private Service() { }

    public static Result<Service> Create(Guid branchId, string name, string description, int durationInMinutes, ServiceType type, Money cost, IServiceSettings serviceSettings)
    {
        if (durationInMinutes % serviceSettings.SlotSizeInMinutes != 0)
            return Result.Failure<Service>(ServiceErrors.DurationNotSlotAligned);

        if (durationInMinutes < serviceSettings.MinimumDurationInMinutes)
            return Result.Failure<Service>(ServiceErrors.MinimumDurationNotMet);

        if (durationInMinutes > serviceSettings.MaximumDurationInMinutes)
            return Result.Failure<Service>(ServiceErrors.MaximumDurationAllowedSurpassed);

        return Result.Success(new Service
        {
            BranchId = branchId,
            Name = name,
            Description = description,
            Type = type,
            Status = ServiceStatus.Active,
            Duration = TimeSpan.FromMinutes(durationInMinutes),
            Cost = cost,
        });
    }

    public Result AddToGallery(MediaFile media, IServiceSettings serviceSettings)
    {
        if (_gallery.Count >= serviceSettings.MaxGallerySize)
            return Result.Failure<Service>(ServiceErrors.MaxGallerySizeReached);

        _gallery.Add(media);
        return Result.Success();
    }

    public Result<bool> RemoveFromGallery(Guid mediaId)
    {
        var item = _gallery.FirstOrDefault(m => m.Id == mediaId);
        return Result.Success(_gallery.Remove(item!));
    }
}
