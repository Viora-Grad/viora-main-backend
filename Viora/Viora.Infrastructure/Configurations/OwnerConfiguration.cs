using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users;

namespace Viora.Infrastructure.Configurations;

internal class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");

        builder.HasKey(owner => owner.Id);

        builder.Property(owner => owner.UserId).IsRequired();
        builder.HasIndex(owner => owner.UserId).IsUnique();
        builder.HasOne(owner => owner.User)
            .WithOne()
            .HasForeignKey<Owner>(owner => owner.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(owner => owner.NationalityId).IsRequired();

        builder.Property(owner => owner.GatewayCredentialsId).IsRequired();

        builder.OwnsOne(owner => owner.AcceptedTerms, acceptedTerms =>
        {
            acceptedTerms.Property(at => at.AcceptedAt).IsRequired().HasColumnName("AcceptedAt");
            acceptedTerms.Property(at => at.Version).IsRequired().HasColumnName("Version");
        });


    }
}
