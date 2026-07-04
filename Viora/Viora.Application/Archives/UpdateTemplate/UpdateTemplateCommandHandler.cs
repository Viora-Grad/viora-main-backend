using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.UpdateTemplate;

internal class UpdateTemplateCommandHandler(
    ITemplateRepository templateRepository) : ICommandHandler<UpdateTemplateCommand>
{
    public async Task<Result> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.Id} not found");

        template.Update(
            new TemplateName(request.Name),
            new TemplateDescription(request.Description));

        templateRepository.Update(template);
        return Result.Success();
    }
}
