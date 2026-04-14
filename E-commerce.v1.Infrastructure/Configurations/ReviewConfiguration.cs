using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.ProductId, x.UserId })
            .IsUnique();

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
