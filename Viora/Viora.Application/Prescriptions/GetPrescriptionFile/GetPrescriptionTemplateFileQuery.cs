using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Prescriptions.GetPrescriptionFile;

public record GetPrescriptionTemplateFileQuery(Guid presceiptionTemplateId) : IQuery<MediaResponseStream>;
