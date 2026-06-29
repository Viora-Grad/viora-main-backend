using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;

namespace Viora.Application.Forms.GetForm;

public record GetFormByIdQuery(Guid FormId) : IQuery<FormResponse>;

