using Viora.Domain.Abstractions;

namespace Viora.Domain.Services;

public static class ServiceErrors
{
    public static readonly Error MaximumDurationAllowedSurpassed =
        new("Services.MaximumDurationAllowedSurpassed", "The allowed duration for service is passed", ErrorCategory.Validation);

    public static readonly Error MinimumDurationNotMet =
        new("Services.MinimumDurationNotMet", "The minimum duration for service is not met", ErrorCategory.Validation);

    public static readonly Error DurationNotSlotAligned =
        new("Services.DurationNotSlotAligned", "Duration must be a multiple of the slot size", ErrorCategory.Validation);

    public static readonly Error MaxGallerySizeReached =
        new("Services.MaxGallerySizeReached", "Max Gallery size reached please update images", ErrorCategory.Validation);

}
