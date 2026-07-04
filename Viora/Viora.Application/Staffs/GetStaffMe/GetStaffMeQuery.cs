using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.GetStaffMe;

// Returns the full profile of the currently authenticated staff member (id taken from the token).
public sealed record GetStaffMeQuery() : IQuery<StaffMeResponse>;
