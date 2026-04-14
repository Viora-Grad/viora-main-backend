using Viora.Domain.Abstractions;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Events;

/// <summary>
/// <strong> This event is made entirely for future purposes when other features are implemented,
/// this could be used to trigger additional actions or notifications. </strong>
/// </summary>
public sealed record OwnerCreatedEvent(Guid Id, Guid UserId, Guid GatewayCredentialsId) : IDomainEvent


