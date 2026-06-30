using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Branches;
using Viora.Domain.Feedbacks.Events;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Feedbacks.FeebackUpdated;

internal class FeedbackUpdatedEventHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository) : INotificationHandler<FeedbackUpdatedEvent>
{
    public async Task Handle(FeedbackUpdatedEvent request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken) ?? throw new NotFoundException($"Branch {request.BranchId} does not exist");

        var organization = await organizationRepository.GetByIdAsync(branch.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization {branch.OrganizationId} NotFound on feedback update");

        var updateResult = organization.UpdateExistingRating(request.RatingOutOfTen);

        if (updateResult.IsFailure)
            throw new ValidationException([new ValidationError(nameof(request.RatingOutOfTen), updateResult.Error.Description)]);
    }
}