using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.GetFormSubmissionFile;

public record GetFormSubmissionFileQuery(Guid FormSubmissionId, Guid FileId) : IQuery<MediaResponseStream>;
