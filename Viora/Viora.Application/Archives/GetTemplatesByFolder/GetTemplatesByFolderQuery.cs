using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetTemplatesByFolder;

public sealed record GetTemplatesByFolderQuery(Guid FolderId) : IQuery<List<TemplateResponse>>;
