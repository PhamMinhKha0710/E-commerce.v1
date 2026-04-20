using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PromotionRuleProductConfiguration : IEntityTypeConfiguration<PromotionRuleProduct>
{
    public void Configure(EntityTypeBuilder<PromotionRuleProduct> builder)
    {
        builder.ToTable("PromotionRuleProducts");
        builder.HasKey(x => new { x.PromotionRuleId, x.ProductId });

        builder.HasOne(x => x.PromotionRule)
            .WithMany(r => r.Products)
            .HasForeignKey(x => x.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProductId);
    }
}

