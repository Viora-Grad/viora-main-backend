using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;

namespace Viora.Application.Plans.CreatePlan;

class CreatePlanCommandHandler(IPlanRepository planRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreatePlanCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var newPlanName = PlanName.Create(request.PlanName);
        var newPlanCode = PlanCode.Create(request.PlanCode);
        var newPlanDescription = PlanDescription.Create(request.PlanDescription);
        var newPlanContent = PlanContent.Create(request.Content);
        var result = Plan.Create(newPlanCode, newPlanName, newPlanDescription, request.Version, newPlanContent, request.ContentForm);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }
        planRepository.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
