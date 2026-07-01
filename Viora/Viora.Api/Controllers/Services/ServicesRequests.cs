namespace Viora.Api.Controllers.Services;

public sealed record AddServiceRequest(
    Guid BranchId,
    string Name,
    string Description,
    string ServiceType,
    int DurationInMinutes,
    decimal CostAmount,
    string Currency);

public sealed record UpdateServiceRequest(
    string Name,
    string Description,
    string ServiceType,
    int DurationInMinutes,
    decimal CostAmount,
    string Currency);

public sealed record AddDiscountRequest(
    int DiscountOutOf100,
    string Reason,
    int DurationInDays);
