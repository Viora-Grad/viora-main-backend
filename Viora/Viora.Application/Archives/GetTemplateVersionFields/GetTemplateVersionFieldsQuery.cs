using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetTemplateVersionFields;

public sealed record GetTemplateVersionFieldsQuery(Guid TemplateId, int Version) : IQuery<TemplateVersionResponse>;
