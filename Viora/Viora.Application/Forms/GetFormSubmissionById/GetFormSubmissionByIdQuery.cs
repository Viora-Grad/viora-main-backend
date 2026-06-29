using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;

namespace Viora.Application.Forms.GetFormSubmissionById;

public record GetFormSubmissionByIdQuery(Guid FormSubmissionId) : IQuery<FormSubmissionResponse>;
