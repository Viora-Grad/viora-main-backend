namespace Viora.Domain.Shared;

public sealed record ServiceType
{
    public string Value { get; init; }

    private ServiceType(string value) => Value = value;

    public static readonly ServiceType GeneralSurgery = new("General Surgery");
    public static readonly ServiceType PsychiatryAndBehavioralHealth = new("Psychiatry & Behavioral Health");
    public static readonly ServiceType Endocrinology = new("Endocrinology");
    public static readonly ServiceType Pediatrics = new("Pediatrics");
    public static readonly ServiceType ObstetricsAndGynecology = new("Obstetrics & Gynecology (OB-GYN)");
    public static readonly ServiceType GastroenterologyAndHepatology = new("Gastroenterology & Hepatology");
    public static readonly ServiceType Cardiology = new("Cardiology");
    public static readonly ServiceType InternalMedicine = new("Internal Medicine");
    public static readonly ServiceType ClinicalPharmacology = new("Clinical Pharmacology");
    public static readonly ServiceType Dermatology = new("Dermatology");
    public static readonly ServiceType OrthopedicSurgery = new("Orthopedic Surgery");
    public static readonly ServiceType Hematology = new("Hematology");
    public static readonly ServiceType Ophthalmology = new("Ophthalmology");
    public static readonly ServiceType ClinicalNutritionAndDietetics = new("Clinical Nutrition & Dietetics");
    public static readonly ServiceType SexualMedicine = new("Sexual Medicine");
    public static readonly ServiceType DentistryAndOralHealth = new("Dentistry & Oral Health");
    public static readonly ServiceType Urology = new("Urology");
    public static readonly ServiceType Otolaryngology = new("Otolaryngology (ENT)");
    public static readonly ServiceType Oncology = new("Oncology");
    public static readonly ServiceType PublicHealthAndPreventiveMedicine = new("Public Health & Preventive Medicine");
    public static readonly ServiceType PlasticAndReconstructiveSurgery = new("Plastic & Reconstructive Surgery");
    public static readonly ServiceType Neurology = new("Neurology");
    public static readonly ServiceType Pulmonology = new("Pulmonology");
    public static readonly ServiceType SportsMedicine = new("Sports Medicine");

    public static ServiceType FromValue(string value) =>
        All.FirstOrDefault(s => s.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Unknown service type: '{value}'.");

    public static IReadOnlyCollection<ServiceType> All => typeof(ServiceType)
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(f => f.FieldType == typeof(ServiceType))
        .Select(f => (ServiceType)f.GetValue(null)!)
        .ToList();
}
