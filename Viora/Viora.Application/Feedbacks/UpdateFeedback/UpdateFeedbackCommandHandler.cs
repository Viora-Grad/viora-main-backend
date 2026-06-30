using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Feedbacks;

namespace Viora.Application.Feedbacks.UpdateFeedback;

internal sealed class UpdateFeedbackCommandHandler(
    IFeedbackRepository feedbackRepository,
    IDateTimeProvider dateTime,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateFeedbackCommand>
{
    public async Task<Result> Handle(UpdateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await feedbackRepository.GetByIdAsync(request.FeedbackId, cancellationToken)
            ?? throw new NotFoundException($"Feedback {request.FeedbackId} not found to update");

        if (feedback.UserId != request.UserId)
            return Result.Failure(FeedbackErrors.UserNotOwnerOfFeedback);

        var result = feedback.Update(request.ServiceRatingOutOfTen, request.BranchOutOfTen, request.SystemExperienceOutOfTen, dateTime.UtcNow, request.Comment);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
