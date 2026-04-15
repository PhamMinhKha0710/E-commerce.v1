using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("PromotionRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(r => new { r.IsActive, r.StartDate, r.EndDate, r.Priority });

        builder.HasOne(r => r.BuyXGetYAction)
            .WithOne(a => a.PromotionRule)
            .HasForeignKey<PromotionBuyXGetYAction>(a => a.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.PercentageAction)
            .WithOne(a => a.PromotionRule)
            .HasForeignKey<PromotionPercentageAction>(a => a.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

