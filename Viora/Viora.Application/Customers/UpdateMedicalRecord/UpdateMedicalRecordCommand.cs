using Viora.Application.Abstractions.Messaging;



namespace Viora.Application.Customers.UpdateMedicalRecord;

public sealed record UpdateMedicalRecordCommand(
    int? Systolic,
    int? Diastolic,
    float? Weight,
    int? HeartRate,
    int? BloodGlucose,
    List<string>? Allergies) : ICommand;

