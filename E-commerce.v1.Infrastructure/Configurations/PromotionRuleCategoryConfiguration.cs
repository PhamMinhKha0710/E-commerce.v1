using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PromotionRuleCategoryConfiguration : IEntityTypeConfiguration<PromotionRuleCategory>
{
    public void Configure(EntityTypeBuilder<PromotionRuleCategory> builder)
    {
        builder.ToTable("PromotionRuleCategories");
        builder.HasKey(x => new { x.PromotionRuleId, x.CategoryId });

        builder.HasOne(x => x.PromotionRule)
            .WithMany(r => r.Categories)
            .HasForeignKey(x => x.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId);
    }
}

