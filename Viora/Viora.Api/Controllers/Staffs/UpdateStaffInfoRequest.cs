namespace Viora.Api.Controllers.Staffs;

public sealed record UpdateStaffInfoRequest(
    string? FirstName,
    string? LastName,
    string? Username,
    string? Password,
    DateOnly? DateOfBirth,
    string? Gender,
    string? PhoneNumber

    );