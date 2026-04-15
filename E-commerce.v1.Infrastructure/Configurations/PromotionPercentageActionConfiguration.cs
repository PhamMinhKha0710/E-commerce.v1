using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PromotionPercentageActionConfiguration : IEntityTypeConfiguration<PromotionPercentageAction>
{
    public void Configure(EntityTypeBuilder<PromotionPercentageAction> builder)
    {
        builder.ToTable("PromotionPercentageActions");
        builder.HasKey(x => x.PromotionRuleId);

        builder.Property(x => x.Percent)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}

