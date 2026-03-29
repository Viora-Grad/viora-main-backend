using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecord.Internal;

namespace Viora.Domain.MedicalRecord;

public sealed class MedicalRecord : Entity
{
    private readonly HashSet<Allergy> _allergies = [];
    public Guid CustomerId { get; private set; }
    public BloodPressure BloodPressure { get; private set; } = null!;
    public Weight Weight { get; private set; } = null!;
    public HeartRate HeartRate { get; private set; } = null!;
    public BloodGlucose BloodGlucose { get; private set; } = null!;
    public IReadOnlyList<Allergy> Allergies => _allergies.ToList().AsReadOnly();


    private MedicalRecord() { } // for ef core

    private MedicalRecord(Guid id,
        Guid customerId,
        BloodPressure bloodPressure,
        Weight weight,
        HeartRate heartRate,
        BloodGlucose bloodGlucose,
        IEnumerable<Allergy> allergies)
        : base(id)
    {
        CustomerId = customerId;
        BloodPressure = bloodPressure;
        Weight = weight;
        HeartRate = heartRate;
        BloodGlucose = bloodGlucose;
        _allergies = [.. allergies];
    }
    public static MedicalRecord Create(Guid id,
        Guid customerId,
        BloodPressure bloodPressure,
        Weight weight,
        HeartRate heartRate,
        BloodGlucose bloodGlucose,
        IEnumerable<Allergy> allergies)
    {
        // add any validation if needed
        return new MedicalRecord(id, customerId, bloodPressure, weight, heartRate, bloodGlucose, allergies);
    }
    public void AddAllergy(Allergy allergy)
    {
        _allergies.Add(allergy);
    }
    public void RemoveAllergy(Allergy allergy)
    {
        _allergies.Remove(allergy);
    }

    public MedicalRecord UpdateMedicalRecord(BloodPressure bloodPressure,
        Weight weight,
        HeartRate heartRate,
        BloodGlucose bloodGlucose,
        IEnumerable<Allergy> allergies) // should this method accept a Medical record instead of individual parameters?
    {
        BloodPressure = bloodPressure;
        Weight = weight;
        HeartRate = heartRate;
        BloodGlucose = bloodGlucose;
        _allergies.Clear();
        _allergies.UnionWith(allergies);

        // TODO: consider raising domain events when triggering this method,
        return this;
    }

}