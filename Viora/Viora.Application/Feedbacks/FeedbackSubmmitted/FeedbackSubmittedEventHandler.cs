using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Feedbacks.Events;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Feedbacks.FeedbackSubmmitted;

internal class FeedbackSubmittedEventHandler(
    IOrganizationRepository organizationRepository) : INotificationHandler<FeedbackSubmittedEvent>
{
    public async Task Handle(FeedbackSubmittedEvent request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} NotFound on feedback update");

        var updateResult = organization.UpdateRating(request.RatingOutOfTen);

        if (updateResult.IsFailure)
            throw new ValidationException([new ValidationError(nameof(request.RatingOutOfTen), updateResult.Error.Description)]);
    }
}
