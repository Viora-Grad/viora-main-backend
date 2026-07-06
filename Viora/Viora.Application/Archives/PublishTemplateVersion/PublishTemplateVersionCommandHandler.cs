using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.PublishTemplateVersion;

internal class PublishTemplateVersionCommandHandler(
    ITemplateRepository templateRepository) : ICommandHandler<PublishTemplateVersionCommand>
{
    public async Task<Result> Handle(PublishTemplateVersionCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.TemplateId} not found");

        var version = template.Versions.FirstOrDefault(v => v.Id == request.VersionId)
            ?? throw new NotFoundException($"Version with id {request.VersionId} not found for template {request.TemplateId}");

        version.Publish();
        templateRepository.Update(template);

        return Result.Success();
    }
}
