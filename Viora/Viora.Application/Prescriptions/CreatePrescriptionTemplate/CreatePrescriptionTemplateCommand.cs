using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Prescriptions.CreatePrescriptionTemplate;

public record CreatePrescriptionTemplateCommand(
    Guid OrganizationId,
    string Name,
    MediaRequest? File,
    double TopMargin,
    double RightMargin,
    double LiftMargin,
    double BottomMarign) : ICommand<Guid>;

