using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.MedicalRecords;
using Viora.Domain.MedicalRecords.Internal;
namespace Viora.Infrastructure.Configurations;

internal class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(medicalRecord => medicalRecord.Id);

        builder.Property(medicalRecord => medicalRecord.CustomerId)
            .IsRequired();
        builder.HasIndex(medicalRecord => medicalRecord.CustomerId)
            .IsUnique();

        builder.Property(medicalRecord => medicalRecord.BloodGlucose)
            .HasConversion(
                bloodGlucose => bloodGlucose.Value,
                value => new BloodGlucose(value))
            .IsRequired();

        builder.Property(medicalRecord => medicalRecord.Weight)
            .HasConversion(
                weight => weight.Value,
                value => new Weight(value))
            .IsRequired();

        builder.Property(medicalRecord => medicalRecord.HeartRate)
            .HasConversion(
                heartRate => heartRate.Value,
                value => new HeartRate(value))
            .IsRequired();

        builder.OwnsOne(medicalRecord => medicalRecord.BloodPressure, bloodPressure =>
        {
            bloodPressure.Property(bp => bp.Systolic)
                .HasColumnName("BloodPressureSystolic")
                .IsRequired();
            bloodPressure.Property(bp => bp.Diastolic)
                .HasColumnName("BloodPressureDiastolic")
                .IsRequired();
        });

        builder.OwnsMany(medicalRecord => medicalRecord.Allergies, allergy =>
        {
            allergy.ToTable("MedicalRecordAllergies");

            allergy.WithOwner().HasForeignKey("MedicalRecordId");

            allergy.Property(a => a.Value)
                .HasColumnName("Allergy")
                .HasMaxLength(200)
                .IsRequired();

            // owned collection must have a key
            allergy.HasKey("MedicalRecordId", "Value");
        });

        builder.Navigation(medicalRecord => medicalRecord.Allergies)
            .HasField("_allergies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(medicalRecord => medicalRecord.Customer)
            .WithOne(customer => customer.MedicalRecord)
            .HasForeignKey<MedicalRecord>(medicalRecord => medicalRecord.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

