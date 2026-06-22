using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Medias;
using Viora.Domain.MedicalRecords;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Internal;

namespace Viora.Infrastructure.Configurations;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.UserName)
            .IsRequired(false)
            .HasConversion(
                userName => userName != null ? userName.Value : null,
                value => value != null ? new UserName(value) : null)
            .HasMaxLength(255);

        builder.OwnsOne(customer => customer.PersonalInfo, personalInfo =>
        {
            personalInfo.Property(info => info.FirstName).HasMaxLength(100).IsRequired();
            personalInfo.Property(info => info.LastName).HasMaxLength(100).IsRequired();
            personalInfo.Property(info => info.DateOfBirth);
            personalInfo.Property(info => info.Gender).HasConversion<string>();
        });
        builder.Property(customer => customer.JoinedAt).IsRequired();

        builder.HasOne(customer => customer.UserProfile)
            .WithOne()
            .HasForeignKey<Customer>(customer => customer.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(customer => customer.OrganizationVisits) // will change when we implement the organization visits feature
            .WithOne(visit => visit.Customer)
            .HasForeignKey(visit => visit.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(customer => customer.PhoneNumbers)
            .HasConversion(
                phoneNumbers => string.Join(";", phoneNumbers.Select(phoneNumber => phoneNumber.Value)),
                value => value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(phoneNumber => new PhoneNumber(phoneNumber)).ToHashSet())
            .HasField("_phoneNumbers")
            .IsRequired(false); // this configuration avoids creating a separate table for phone numbers and stores them as a semicolon-separated string in the Customers table

        builder.Property(customer => customer.Emails)
            .HasConversion(
                emails => string.Join(";", emails.Select(email => email.Value)),
                value => value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(email => new Domain.Users.Internal.Email(value)).ToHashSet())
            .HasField("_contactEmails")
            .IsRequired(false); // this configuration avoids creating a separate table for emails and stores them as a semicolon-separated string in the Customers table

        builder.Property(customer => customer.MedicalRecordId).IsRequired(false);
        builder.HasOne(customer => customer.MedicalRecord)
            .WithOne(medicalRecord => medicalRecord.Customer)
            .HasForeignKey<MedicalRecord>(medicalRecord => medicalRecord.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(customer => customer.ProfilePicId).IsRequired(false);
        builder.HasOne<MediaFile>()
            .WithOne()
            .HasForeignKey<Customer>(customer => customer.ProfilePicId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
