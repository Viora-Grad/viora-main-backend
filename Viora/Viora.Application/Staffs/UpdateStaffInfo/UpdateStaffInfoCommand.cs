using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.UpdateStaffInfo;

public sealed record UpdateStaffInfoCommand(
    Guid StaffId,
    string? FirstName,
    string? LastName,
    string? Username,
    string? Password,
    DateOnly? DateOfBirth,
    string? Gender,
    string? PhoneNumber
) : ICommand;
