using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PromotionDiscount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CouponDiscount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RankDiscount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.CouponCode)
            .HasMaxLength(64);

        builder.Property(x => x.PromotionSummary)
            .HasMaxLength(1024);

        builder.HasOne(x => x.PromotionRule)
            .WithMany()
            .HasForeignKey(x => x.PromotionRuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AhamoveOrderId).HasMaxLength(64);
        builder.HasIndex(x => x.AhamoveOrderId).IsUnique().HasFilter("[AhamoveOrderId] IS NOT NULL");

        builder.Property(x => x.AhamoveLastStatus).HasMaxLength(64);

        builder.Property(x => x.ShippingReceiverName).HasMaxLength(128);
        builder.Property(x => x.ShippingReceiverPhone).HasMaxLength(32);
        builder.Property(x => x.ShippingAddressLine).HasMaxLength(512);
        builder.Property(x => x.ShippingNote).HasMaxLength(512);
        builder.Property(x => x.ShippingServiceId).HasMaxLength(64);
        builder.Property(x => x.ShippingFee).HasColumnType("decimal(18,2)");
    }
}