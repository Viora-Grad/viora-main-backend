using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.GetMetaCredentialStatus;

// Reports whether the caller's organization has a Facebook Page credential saved.
public sealed record GetMetaCredentialStatusQuery : IQuery<MetaCredentialStatusResponse>;
