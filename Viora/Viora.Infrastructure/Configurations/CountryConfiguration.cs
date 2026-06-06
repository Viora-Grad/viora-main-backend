using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Shared;

namespace Viora.Infrastructure.Configurations;

internal class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property<string>(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);


        builder.Property<string>(a => a.IsoAlphaThree)
            .IsRequired()
            .HasMaxLength(50);


        builder.Property<string>(a => a.Nationality)
            .IsRequired()
            .HasMaxLength(255);

    }
}
/*    public string Name { get; private set; } = default!;
    /// <summary>
    /// Country code resembles the 3 characters of country like USA
    /// </summary>
    public string IsoAlphaThree { get; private set; } = default!;
    public string Nationality { get; private set; } = default!;*/