using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Feedbacks;

namespace Viora.Application.Feedbacks.AddFeedback;

internal sealed class AddFeedbackCommandHandler(
    IFeedbackRepository feedbackRepository,
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTime,
    IUnitOfWork unitOfWork) : ICommandHandler<AddFeedbackCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedbackUser = await feedbackRepository.GetByUserIdAsync(request.UserId, request.BranchId, cancellationToken);

        if (feedbackUser != null)
            return Result.Failure<Guid>(FeedbackErrors.AlreadyRated);

        var userHasAppointemnts = await appointmentsRepository.UserHasAppointemtns(request.UserId, request.BranchId, cancellationToken);
        if (!userHasAppointemnts)
            return Result.Failure<Guid>(FeedbackErrors.UserHasNoAppointmentsInBranch);

        var feedbackResult = Feedback.Create(
                request.BranchId,
                request.UserId,
                request.ServiceRatingOutOfTen,
                request.BranchOutOfTen,
                request.SystemExperienceOutOfTen,
                dateTime.UtcNow,
                request.Comment);

        if (feedbackResult.IsFailure)
            return Result.Failure<Guid>(feedbackResult.Error);

        feedbackRepository.Add(feedbackResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(feedbackResult.Value.Id);
    }
}
