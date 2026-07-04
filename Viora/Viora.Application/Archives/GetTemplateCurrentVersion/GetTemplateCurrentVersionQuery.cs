using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetTemplateCurrentVersion;

public sealed record GetTemplateCurrentVersionQuery(Guid TemplateId) : IQuery<TemplateVersionResponse>;
