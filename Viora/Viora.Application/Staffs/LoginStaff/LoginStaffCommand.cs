using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.LoginStaff;

public sealed record LoginStaffCommand(Guid OrganizationId, string Username, string Password) : ICommand<AuthResult>;
