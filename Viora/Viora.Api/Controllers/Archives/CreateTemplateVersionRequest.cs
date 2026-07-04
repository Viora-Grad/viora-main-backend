using Viora.Application.Archives.Shared;

namespace Viora.Api.Controllers.Archives;

public sealed record CreateTemplateVersionRequest(
    List<TemplateFieldDto> Fields
);
