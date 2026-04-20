using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.MedicalRecords;
using Viora.Domain.Users.Customers;

namespace Viora.Infrastructure.Configurations;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.UserId).IsRequired();
        builder.HasIndex(customer => customer.UserId).IsUnique();
        builder.HasOne(customer => customer.User)
            .WithOne()
            .HasForeignKey<Customer>(customer => customer.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(customer => customer.MedicalRecordId).IsRequired(false);
        builder.HasOne(customer => customer.MedicalRecord)
            .WithOne(medicalRecord => medicalRecord.Customer)
            .HasForeignKey<MedicalRecord>(medicalRecord => medicalRecord.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);


    }
}
