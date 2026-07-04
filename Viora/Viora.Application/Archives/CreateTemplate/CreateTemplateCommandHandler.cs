using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateTemplate;

internal class CreateTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateTemplateCommand, TemplateResponse>
{
    public async Task<Result<TemplateResponse>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = Template.Create(
            request.ArchiveId,
            request.FolderId,
            new TemplateName(request.Name),
            new TemplateDescription(request.Description),
            dateTimeProvider.UtcNow);

        templateRepository.Add(template);

        var response = new TemplateResponse(
            template.Id,
            template.ArchiveId,
            template.FolderId,
            template.Name.Value,
            template.Description.Value,
            template.CurrentVersion,
            template.CreatedAt);

        return Result.Success(response);
    }
}
