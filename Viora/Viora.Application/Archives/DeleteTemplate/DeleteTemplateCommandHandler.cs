using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.DeleteTemplate;

internal class DeleteTemplateCommandHandler(
    ITemplateRepository templateRepository) : ICommandHandler<DeleteTemplateCommand>
{
    public async Task<Result> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.Id} not found");

        templateRepository.Remove(template);
        return Result.Success();
    }
}
