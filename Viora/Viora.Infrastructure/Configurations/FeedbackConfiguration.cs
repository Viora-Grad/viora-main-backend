using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Branches;
using Viora.Domain.Feedbacks;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Configurations;

internal class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable(nameof(Feedback));

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever();

        builder.Property(f => f.BranchId)
            .IsRequired();

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.ComplexProperty(f => f.Ratings, rb =>
        {
            rb.Property(r => r.ServiceOutOfTen).HasColumnName("RatingService").IsRequired();
            rb.Property(r => r.BranchOutOfTen).HasColumnName("RatingBranch").IsRequired();
            rb.Property(r => r.SystemOutOfTen).HasColumnName("RatingSystem").IsRequired();
        });

        builder.OwnsOne(f => f.Comment, cb =>
        {
            cb.Property(c => c.Value)
                .HasColumnName("Comment")
                .HasMaxLength(1000);
        });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);      // deleting the user also deletes his feedbacks

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);     //has to be maunally deleted on organization termination since multiple cascade path is not allowed
    }
}
