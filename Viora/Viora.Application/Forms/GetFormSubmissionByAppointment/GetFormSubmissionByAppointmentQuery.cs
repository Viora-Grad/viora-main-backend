using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;

namespace Viora.Application.Forms.GetFormSubmissionByAppointment;

public record GetFormSubmissionByAppointmentQuery(Guid AppointmentId, Guid FormId) : IQuery<FormSubmissionResponse>;
