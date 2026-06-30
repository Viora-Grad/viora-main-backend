using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.RegisterStaff;

public sealed record RegisterStaffCommand(
    Guid OrganizationId,
    string Token,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Username,
    string Password
) : ICommand<Guid>;
