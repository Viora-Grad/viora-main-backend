using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Configurations;

internal sealed class MetaPageCredentialConfiguration : IEntityTypeConfiguration<MetaPageCredential>
{
    public void Configure(EntityTypeBuilder<MetaPageCredential> builder)
    {
        builder.ToTable("MetaPageCredentials");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.OrganizationId).IsRequired();
        builder.Property(c => c.PageId).HasMaxLength(100).IsRequired();

        // Stores the ICipher-encrypted (Base64) token; default nvarchar(max) leaves room for long ciphertext.
        builder.Property(c => c.AccessToken).IsRequired();

        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc).IsRequired();

        // At most one active Facebook Page credential per organization.
        builder.HasIndex(c => c.OrganizationId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}
