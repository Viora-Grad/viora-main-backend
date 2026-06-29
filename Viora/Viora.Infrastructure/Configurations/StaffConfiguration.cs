using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Configurations;

internal class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.HasMany(st => st.Branches)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "StaffBranch",
                j => j.HasOne<Branch>().WithMany().HasForeignKey("BranchId"),
                j => j.HasOne<Staff>().WithMany().HasForeignKey("StaffId")
            );
        builder.HasMany(st => st.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "StaffRole",
                j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                j => j.HasOne<Staff>().WithMany().HasForeignKey("StaffId")
            );

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.FirstName)
            .HasConversion(
                v => v.Value,
                v => new FirstName(v))
            .IsRequired(false);

        builder.Property(x => x.LastName)
            .HasConversion(
                v => v.Value,
                v => new LastName(v))
            .IsRequired(false);

        builder.Property(x => x.Username)
            .HasConversion(
                v => v.Value,
                v => new Username(v))
            .IsRequired(false);

        builder.Property(x => x.HashedPassword)
            .HasConversion(
                v => v.Value,
                v => new HashedPassword(v))
            .IsRequired(false);

        builder.Property(x => x.DateOfBirth);
        builder.Property(x => x.StaffStatus)
            .HasConversion<string>();

        builder.Property(x => x.Gender)
            .HasConversion<string>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);

        builder.Ignore(x => x.IsDeleted);

        builder.Property(x => x.PhoneNumber)
            .HasConversion(
                v => v.Value,
                v => new PhoneNumber(v))
            .IsRequired(false);

        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.OrganizationId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
