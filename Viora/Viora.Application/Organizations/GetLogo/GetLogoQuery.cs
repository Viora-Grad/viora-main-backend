using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.GetLogo;

public sealed record GetLogoQuery(Guid OrganizationId) : IQuery<MediaResponseStream>;