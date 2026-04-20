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

        builder.Property(user => user.FirstName)
            .HasConversion(firstName => firstName.Value, value => new FirstName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasConversion(lastName => lastName.Value, value => new LastName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.UserName)
            .HasConversion(userName => userName.Value, value => new UserName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(user => user.UserName).IsUnique();

        builder.Property(user => user.Age)
            .HasConversion(age => age.Value, value => new Age(value))
            .IsRequired();

        builder.Property(user => user.JoinedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(user => user.IsEmailVerified);

        builder.HasMany(user => user.Identities)
            .WithOne(identity => identity.User)
            .HasForeignKey(identity => identity.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.Identities)
            .HasField("_identities")
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        builder.OwnsMany(user => user.Contacts, b =>
        {
            b.ToTable("UserContacts");
            b.WithOwner().HasForeignKey("UserId");

            b.Property<int>("Id");
            b.HasKey("Id");

            b.OwnsOne(contact => contact.PhoneNumber, pb =>
            {
                pb.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(32).IsRequired();
            });

            b.OwnsOne(contact => contact.Email, eb =>
            {
                eb.Property(e => e.Value).HasColumnName("ContactEmail").HasMaxLength(256).IsRequired(); // Column name set to ContactEmail to avoid confusion with User's Email
            });
        });

        builder.Navigation(user => user.Contacts)
            .HasField("_contact")
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
