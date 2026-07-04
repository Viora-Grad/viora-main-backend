using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetTemplate;

public sealed record GetTemplateQuery(Guid Id) : IQuery<TemplateResponse>;
