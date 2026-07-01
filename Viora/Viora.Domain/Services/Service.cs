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
    public ServiceType Type { get; private set; } = default!;
    public ServiceStatus Status { get; private set; }

    public TimeSpan Duration { get; private set; }
    public Money Cost { get; private set; } = default!;

    // TODO for better UX add description per image for what this image represents, not prio now, done later (dunno when xd)
    private readonly List<MediaFile> _gallery = [];
    public IReadOnlyCollection<MediaFile> Gallery => _gallery.AsReadOnly();

    public Discount? Discount { get; set; } = null;

    private Service() { }

    public static Result<Service> Create(Guid branchId, string name, string description, int durationInMinutes, ServiceType type, Money cost, IServiceSettings serviceSettings)
    {
        var durationValidation = ValidateDuration(durationInMinutes, serviceSettings);
        if (durationValidation.IsFailure)
            return Result.Failure<Service>(durationValidation.Error);

        return Result.Success(new Service
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Name = name,
            Description = description,
            Type = type,
            Status = ServiceStatus.Active,
            Duration = TimeSpan.FromMinutes(durationInMinutes),
            Cost = cost,
        });
    }

    /// <summary>
    /// Updates the mutable details of the service. The service <see cref="Type"/> must still be a
    /// specialty offered by the owning organization; that check is enforced by the application layer.
    /// </summary>
    public Result Update(string name, string description, int durationInMinutes, ServiceType type, Money cost, IServiceSettings serviceSettings)
    {
        var durationValidation = ValidateDuration(durationInMinutes, serviceSettings);
        if (durationValidation.IsFailure)
            return durationValidation;

        Name = name;
        Description = description;
        Type = type;
        Duration = TimeSpan.FromMinutes(durationInMinutes);
        Cost = cost;

        return Result.Success();
    }

    private static Result ValidateDuration(int durationInMinutes, IServiceSettings serviceSettings)
    {
        if (durationInMinutes % serviceSettings.SlotSizeInMinutes != 0)
            return Result.Failure(ServiceErrors.DurationNotSlotAligned);

        if (durationInMinutes < serviceSettings.MinimumDurationInMinutes)
            return Result.Failure(ServiceErrors.MinimumDurationNotMet);

        if (durationInMinutes > serviceSettings.MaximumDurationInMinutes)
            return Result.Failure(ServiceErrors.MaximumDurationAllowedSurpassed);

        return Result.Success();
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

    public Result AddDiscount(int discountOutOf100, string reason, TimeSpan duration, DateTime currentDateTime)
    {
        if (discountOutOf100 > 100 || discountOutOf100 < 0)
            return Result.Failure(ServiceErrors.DiscountRangeUnallowed);

        var discount = new Discount(discountOutOf100, reason, currentDateTime, currentDateTime + duration);
        Discount = discount;

        return Result.Success();
    }

    /// <summary>Clears the active discount. Invoked when the scheduled <c>DiscountEndedEvent</c> fires.</summary>
    public void EndDiscount() => Discount = null;
}
