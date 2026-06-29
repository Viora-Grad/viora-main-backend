using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;

namespace Viora.Application.Forms.GetServiceForm;

public record GetServiceFormQuery(Guid ServiceId) : IQuery<FormResponse>;

