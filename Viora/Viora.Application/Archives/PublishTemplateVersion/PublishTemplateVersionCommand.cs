using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.PublishTemplateVersion;

public sealed record PublishTemplateVersionCommand(Guid TemplateId, Guid VersionId) : ICommand;
