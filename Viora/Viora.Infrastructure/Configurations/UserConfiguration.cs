using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);


        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();


        builder.Property(user => user.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(user => user.LastLoginAt)
            .HasColumnType("datetime");

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.OwnsOne(user => user.PersonalInfo, owned =>
        {
            owned.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            owned.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            owned.Property(p => p.DateOfBirth).IsRequired();
            owned.Property(p => p.Gender).HasConversion<string>().IsRequired();
        });

        builder.Property(user => user.IsEmailVerified);

        builder.HasMany(user => user.Identities)
            .WithOne(identity => identity.User)
            .HasForeignKey(identity => identity.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.Identities)
            .HasField("_identities")
            .UsePropertyAccessMode(PropertyAccessMode.Field);



        builder.HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserRole",
                j => j.HasOne<Role>().WithMany()
                  .HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany()
                  .HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("UserId", "RoleId")
            );

        builder.Navigation(user => user.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
