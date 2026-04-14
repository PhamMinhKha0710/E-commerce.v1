using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_commerce.v1.Domain.Entities;
namespace E_commerce.v1.Infrastructure.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AppliedCouponCode)
            .HasMaxLength(64);

        builder.Property(c => c.CouponDiscountPreview)
            .HasColumnType("decimal(18,2)");

        // add relationship between Cart and User
        builder.HasOne(c => c.User)
               .WithOne()
               .HasForeignKey<Cart>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AppliedCoupon)
            .WithMany()
            .HasForeignKey(c => c.AppliedCouponId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}