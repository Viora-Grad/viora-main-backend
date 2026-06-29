using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Identity;
namespace Viora.Infrastructure.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        //builder.HasKey(r => r.Id);
        builder.Property(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>();

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(r => r.TenantId)
            .IsRequired(false);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
