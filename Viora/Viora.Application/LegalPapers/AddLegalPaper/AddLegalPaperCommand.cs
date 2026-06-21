using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Application.LegalPapers.AddLegalPaper;

public sealed record AddLegalPaperCommand(Guid ApplicationId, Guid UserId, MediaRequest MediaContent, LegalPaperType Type, string OfficalName, DateTime ExpiryDateUtc) : ICommand<Guid>;