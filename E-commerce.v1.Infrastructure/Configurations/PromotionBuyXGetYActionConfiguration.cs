using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PromotionBuyXGetYActionConfiguration : IEntityTypeConfiguration<PromotionBuyXGetYAction>
{
    public void Configure(EntityTypeBuilder<PromotionBuyXGetYAction> builder)
    {
        builder.ToTable("PromotionBuyXGetYActions");
        builder.HasKey(x => x.PromotionRuleId);

        builder.Property(x => x.BuyQty).IsRequired();
        builder.Property(x => x.GetQty).IsRequired();

        builder.HasOne(x => x.BuyProduct)
            .WithMany()
            .HasForeignKey(x => x.BuyProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GetProduct)
            .WithMany()
            .HasForeignKey(x => x.GetProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BuyCategory)
            .WithMany()
            .HasForeignKey(x => x.BuyCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GetCategory)
            .WithMany()
            .HasForeignKey(x => x.GetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

