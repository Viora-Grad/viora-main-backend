using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.UploadFormSubmissionFile;

public record UploadFormSubmissionFileCommand(Guid FormSubmissionId, MediaRequest File) : ICommand;

