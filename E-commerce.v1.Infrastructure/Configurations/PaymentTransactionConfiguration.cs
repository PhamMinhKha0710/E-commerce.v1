using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.v1.Infrastructure.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(8);
        builder.Property(x => x.ProviderPaymentLinkId).HasMaxLength(128);
        builder.Property(x => x.LastEventIdempotencyKey).HasMaxLength(128);
        builder.Property(x => x.RawLastWebhookPayload).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Provider, x.ProviderOrderCode })
            .HasFilter("[ProviderOrderCode] IS NOT NULL");

        builder.HasIndex(x => new { x.Provider, x.ProviderPaymentLinkId })
            .HasFilter("[ProviderPaymentLinkId] IS NOT NULL");
    }
}

