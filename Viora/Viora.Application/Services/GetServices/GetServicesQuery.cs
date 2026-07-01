using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Services.GetServices;

public sealed record GetServicesQuery(Guid BranchId) : IQuery<IReadOnlyCollection<GetServicesResponse>>;
