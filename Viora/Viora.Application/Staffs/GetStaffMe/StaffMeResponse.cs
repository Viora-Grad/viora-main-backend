namespace Viora.Application.Staffs.GetStaffMe;

public sealed record StaffMeResponse(
    Guid Id,
    Guid OrganizationId,
    string? FirstName,
    string? LastName,
    string? Username,
    string? PhoneNumber,
    string? Gender,
    DateOnly? DateOfBirth,
    string Status,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<StaffRoleResponse> Roles,
    IReadOnlyCollection<StaffBranchResponse> Branches,
    IReadOnlyCollection<StaffServiceResponse> Services);

public sealed record StaffRoleResponse(
    int Id,
    string Name,
    string? Description,
    IReadOnlyCollection<StaffPermissionResponse> Permissions);

public sealed record StaffPermissionResponse(int Id, string Name, string? Description);

public sealed record StaffBranchResponse(
    Guid Id,
    Guid OrganizationId,
    string Status,
    StaffBranchAddressResponse Address,
    string ContactEmail,
    string TimeZone,
    double Latitude,
    double Longitude,
    DateTime OpenedAtUtc,
    IReadOnlyCollection<string> ServicesProvided,
    IReadOnlyCollection<string> PhoneNumbers,
    IReadOnlyCollection<StaffBusinessHourResponse> BusinessHours);

public sealed record StaffBranchAddressResponse(
    int Number,
    string Street,
    string City,
    string State,
    Guid CountryId,
    int PostalCode);

public sealed record StaffBusinessHourResponse(string Day, TimeSpan OpenTime, TimeSpan CloseTime);

public sealed record StaffServiceResponse(
    Guid Id,
    Guid BranchId,
    string Name,
    string Description,
    string Type,
    string Status,
    int DurationMinutes,
    decimal Cost,
    string Currency,
    StaffServiceDiscountResponse? Discount);

public sealed record StaffServiceDiscountResponse(
    int PercentageOutOf100,
    string Reason,
    DateTime StartDateUtc,
    DateTime EndDateUtc);
