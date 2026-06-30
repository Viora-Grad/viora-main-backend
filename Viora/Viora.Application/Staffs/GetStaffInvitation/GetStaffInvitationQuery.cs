using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.GetStaffInvitation;

public sealed record GetStaffInvitationQuery(Guid InvitationId) : IQuery<StaffToken>;