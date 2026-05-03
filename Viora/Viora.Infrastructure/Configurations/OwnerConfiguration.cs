using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Configurations;

internal class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");

        builder.HasKey(owner => owner.Id);
        builder.HasOne(owner => owner.UserProfile)
            .WithOne()
            .HasForeignKey<Owner>(owner => owner.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(owner => owner.PersonalInfo, personalInfo =>
        {
            personalInfo.Property(info => info.FirstName).HasColumnName("FirstName").IsRequired();
            personalInfo.Property(info => info.LastName).HasColumnName("LastName").IsRequired();
            personalInfo.Property(info => info.DateOfBirth).HasColumnName("DateOfBirth").IsRequired();
            personalInfo.Property(info => info.Gender).HasColumnName("Gender").HasConversion<string>().IsRequired();
        });
        builder.Property(owner => owner.NationalityId).IsRequired(); // relation will be configured in NationalityConfiguration
        builder.Property(owner => owner.BecameOwnerAt).IsRequired();


    }
}
