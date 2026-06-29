using System.Text.Json;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.SubmitFormAnswer;

public record SubmitFormAnswerCommand(Guid AppointmentId, Guid FormId, JsonDocument Submission, List<MediaRequest> MediaRequests) : ICommand;

