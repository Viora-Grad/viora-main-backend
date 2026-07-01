using Viora.Application.AiRag.Abstractions;
using Viora.Domain.AiRag;
using Viora.Domain.Users.Customers;

namespace Viora.Infrastructure.AiRag;

internal sealed class UserProfileService : IUserProfileService
{
    private readonly ICustomerRepository _customerRepository;

    public UserProfileService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<UserContext?> GetUserContextAsync(Guid userId, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(userId, ct);
        if (customer is null) return null;

        var firstName = customer.PersonalInfo.FirstName;

        string? medicalRecordSummary = null;
        if (customer.MedicalRecord is not null)
        {
            var mr = customer.MedicalRecord;
            var parts = new List<string>();

            parts.Add($"Blood Pressure: {mr.BloodPressure.Systolic}/{mr.BloodPressure.Diastolic} mmHg");
            parts.Add($"Weight: {mr.Weight.Value} kg");
            parts.Add($"Heart Rate: {mr.HeartRate.Value} bpm");
            parts.Add($"Blood Glucose: {mr.BloodGlucose.Value} mg/dL");

            if (mr.Allergies.Count > 0)
                parts.Add($"Allergies: {string.Join(", ", mr.Allergies.Select(a => a.Value))}");

            medicalRecordSummary = string.Join(" | ", parts);
        }

        return new UserContext(firstName, medicalRecordSummary);
    }
}
