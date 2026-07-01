namespace Viora.Application.Services.GetServices;

public sealed record GetServicesResponse(
    Guid Id,
    Guid BranchId,
    string Name,
    string Description,
    string ServiceType,
    string Status,
    int DurationMinutes,
    decimal Cost,
    string Currency,
    DiscountResponse? Discount);

public sealed record DiscountResponse(
    int PercentageOutOf100,
    string Reason,
    DateTime StartDateUtc,
    DateTime EndDateUtc);
