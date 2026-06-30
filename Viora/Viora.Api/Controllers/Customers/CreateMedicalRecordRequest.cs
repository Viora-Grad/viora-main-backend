namespace Viora.Api.Controllers.Customers;

public sealed record CreateMedicalRecordRequest(
    int Systolic,
    int Diastolic,
    float Weight,
    int HeartRate,
    int BloodGlucose,
    IEnumerable<string> Allergies
    );
