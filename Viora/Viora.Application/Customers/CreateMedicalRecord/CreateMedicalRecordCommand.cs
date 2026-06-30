using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Customers.CreateMedicalRecord;

public sealed record CreateMedicalRecordCommand(
    int Systolic,
    int Diastolic,
    float Weight,
    int HeartRate,
    int BloodGlucose,
    List<string> Allergies
    ) : ICommand<Guid>;
