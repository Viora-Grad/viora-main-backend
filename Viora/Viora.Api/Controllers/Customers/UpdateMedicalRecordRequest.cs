namespace Viora.Api.Controllers.Customers;

public sealed record UpdateMedicalRecordRequest(
    int? Systolic,
    int? Diastolic,
    float? Weight,
    int? HeartRate,
    int? BloodGlucose,
    IEnumerable<string> Allergies
);