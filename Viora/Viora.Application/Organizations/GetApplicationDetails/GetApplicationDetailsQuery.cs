using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.GetApplicationDetails;

public sealed record GetApplicationDetailsQuery(Guid? Id = null, Guid? OwnerId = null) : IQuery<ApplicationDetailsResponse>;