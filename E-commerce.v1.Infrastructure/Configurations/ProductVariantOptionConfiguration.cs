using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class ProductVariantOptionConfiguration : IEntityTypeConfiguration<ProductVariantOption>
{
    public void Configure(EntityTypeBuilder<ProductVariantOption> builder)
    {
        builder.ToTable("ProductVariantOptions");

        builder.HasKey(o => new { o.VariantId, o.Key });

        builder.Property(o => o.Key)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(o => o.Value)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(o => o.VariantId);

        builder.HasOne(o => o.Variant)
            .WithMany(v => v.Options)
            .HasForeignKey(o => o.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

