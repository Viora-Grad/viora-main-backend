using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.LegalPapers.Events;

public sealed record LegalPaperExpiredDomainEvent(Guid paperId) : IDomainEvent;
