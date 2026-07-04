using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.GetStaffById;

public sealed record GetStaffByIdQuery(Guid Id) : IQuery<GetStaffByIdResponse>;

