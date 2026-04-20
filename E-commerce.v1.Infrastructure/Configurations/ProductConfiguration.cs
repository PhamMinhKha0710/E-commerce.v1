using System.Text.Json;
using E_commerce.v1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace E_commerce.v1.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.NameEn)
            .HasMaxLength(255);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Discount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Unit)
            .HasMaxLength(64);

        builder.Property(p => p.UnitEn)
            .HasMaxLength(64);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        var docIdsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>());

        var docIdsComparer = new ValueComparer<List<string>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
            v => v.ToList());

        builder.Property(p => p.DocumentIds)
            .HasConversion(docIdsConverter)
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(docIdsComparer);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
