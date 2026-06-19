using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Application.LegalPapers.UpdateLegalPaperStatus;

internal class UpdateLegalPaperStatusCommandHandler(
    ILegalPaperRepository legalPaperRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateLegalPaperStatusCommand>
{
    public async Task<Result> Handle(UpdateLegalPaperStatusCommand request, CancellationToken cancellationToken)
    {
        var legalPaper = await legalPaperRepository.GetByIdAsync(request.LegalPaperId, cancellationToken)
            ?? throw new NotFoundException($"Legal Paper with Id {request.LegalPaperId} not found");

        if (legalPaper.Status != AcceptanceStatus.UnderReview)
            return Result.Failure(LegalPaperErrors.PaperStatusNotUnderReview);

        var result = request.Status switch
        {
            AcceptanceStatus.Accepted => legalPaper.Accept(dateTimeProvider.UtcNow),
            AcceptanceStatus.Denied => legalPaper.Deny(dateTimeProvider.UtcNow),
            _ => Result.Failure(LegalPaperErrors.PaperStatusNotUnderReview)
        };

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
