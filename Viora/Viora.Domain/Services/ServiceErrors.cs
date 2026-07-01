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

    public static readonly Error DiscountRangeUnallowed =
        new("Services.DiscountRangeUnallowed", "Discount must be between 0 and 100", ErrorCategory.Validation);

    public static readonly Error NotFound =
        new("Services.NotFound", "The requested service was not found", ErrorCategory.NotFound);

    public static readonly Error UnknownServiceType =
        new("Services.UnknownServiceType", "The provided service type is not a recognized specialty", ErrorCategory.Validation);

    public static readonly Error ServiceTypeNotOfferedByOrganization =
        new("Services.ServiceTypeNotOfferedByOrganization", "The service type is not among the specialties the organization provides", ErrorCategory.Validation);
}
