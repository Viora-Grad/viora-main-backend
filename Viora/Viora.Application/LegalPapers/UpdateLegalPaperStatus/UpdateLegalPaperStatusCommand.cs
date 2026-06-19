using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Application.LegalPapers.UpdateLegalPaperStatus;

public sealed record UpdateLegalPaperStatusCommand(Guid LegalPaperId, AcceptanceStatus Status) : ICommand;
