using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Features.Products.Queries.GetProductById;
using E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;
using E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;
using E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Tests;

public class VariantHandlersTests
{
    [Fact]
    public async Task should_create_variant_when_product_exists()
    {
        await using var context = CreateContext();
        var productId = await SeedProduct(context);

        var variantRepository = new VariantRepository(context);
        var productRepository = new ProductQueryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var handler = new CreateVariantCommandHandler(variantRepository, productRepository, unitOfWork);
        var variantId = await handler.Handle(
            new CreateVariantCommand(
                ProductId: productId,
                Sku: "SKU-V-1",
                Price: 120,
                Inventory: 5,
                IsActive: true,
                Options: new List<ProductVariantOptionDto>
                {
                    new() { Key = "size", Value = "M" },
                    new() { Key = "color", Value = "red" }
                }),
            CancellationToken.None);

        var saved = await context.ProductVariants
            .Include(v => v.Options)
            .SingleAsync(v => v.Id == variantId);

        Assert.Equal(productId, saved.ProductId);
        Assert.Equal("SKU-V-1", saved.Sku);
        Assert.Equal(120, saved.Price);
        Assert.Equal(5, saved.Inventory);
        Assert.True(saved.IsActive);
        Assert.Equal(2, saved.Options.Count);
        Assert.Contains(saved.Options, o => o.Key == "size" && o.Value == "M");
        Assert.Contains(saved.Options, o => o.Key == "color" && o.Value == "red");
    }

    [Fact]
    public async Task should_reject_create_variant_when_sku_exists()
    {
        await using var context = CreateContext();
        var productId = await SeedProduct(context);

        context.ProductVariants.Add(new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = "DUP-SKU",
            Price = 10,
            Inventory = 1,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var variantRepository = new VariantRepository(context);
        var productRepository = new ProductQueryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var handler = new CreateVariantCommandHandler(variantRepository, productRepository, unitOfWork);
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(
            new CreateVariantCommand(productId, "DUP-SKU", 99, 10, true, Options: null),
            CancellationToken.None));
    }

    [Fact]
    public async Task should_include_variants_in_product_detail()
    {
        await using var context = CreateContext();
        var productId = await SeedProduct(context);
        var variantId = Guid.NewGuid();

        context.ProductVariants.Add(new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Sku = "SKU-DETAIL-1",
            Price = 200,
            Inventory = 7,
            IsActive = true,
            Options = new List<ProductVariantOption>
            {
                new() { VariantId = variantId, Key = "size", Value = "L" }
            }
        });
        await context.SaveChangesAsync();

        var productRepository = new ProductReadRepository(context);
        var handler = new GetProductByIdQueryHandler(productRepository);
        var result = await handler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Variants);
        Assert.Equal("SKU-DETAIL-1", result.Variants[0].Sku);
        Assert.Single(result.Variants[0].Options);
        Assert.Equal("size", result.Variants[0].Options[0].Key);
        Assert.Equal("L", result.Variants[0].Options[0].Value);
    }

    [Fact]
    public async Task should_hide_deleted_variant_in_product_detail()
    {
        await using var context = CreateContext();
        var productId = await SeedProduct(context);

        var variantId = Guid.NewGuid();
        context.ProductVariants.Add(new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Sku = "SKU-DEL-1",
            Price = 50,
            Inventory = 2,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var variantRepository = new VariantRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var deleteHandler = new DeleteVariantCommandHandler(variantRepository, unitOfWork);
        await deleteHandler.Handle(new DeleteVariantCommand(variantId), CancellationToken.None);

        var productRepository = new ProductReadRepository(context);
        var detailHandler = new GetProductByIdQueryHandler(productRepository);
        var result = await detailHandler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        Assert.Empty(result.Variants);
    }

    [Fact]
    public async Task should_update_variant_and_replace_options()
    {
        await using var context = CreateContext();
        var productId = await SeedProduct(context);
        var variantId = Guid.NewGuid();

        context.ProductVariants.Add(new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Sku = "SKU-UPD-1",
            Price = 10,
            Inventory = 1,
            IsActive = true,
            Options = new List<ProductVariantOption>
            {
                new() { VariantId = variantId, Key = "size", Value = "S" }
            }
        });
        await context.SaveChangesAsync();

        var variantRepository = new VariantRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var updateHandler = new UpdateVariantCommandHandler(variantRepository, unitOfWork);
        await updateHandler.Handle(
            new UpdateVariantCommand(
                Id: variantId,
                Sku: "SKU-UPD-2",
                Price: 15,
                Inventory: 3,
                IsActive: false,
                Options: new List<ProductVariantOptionDto>
                {
                    new() { Key = "color", Value = "blue" }
                }),
            CancellationToken.None);

        var saved = await context.ProductVariants
            .Include(v => v.Options)
            .SingleAsync(v => v.Id == variantId);

        Assert.Equal("SKU-UPD-2", saved.Sku);
        Assert.Equal(15, saved.Price);
        Assert.Equal(3, saved.Inventory);
        Assert.False(saved.IsActive);
        Assert.Single(saved.Options);
        Assert.Equal("color", saved.Options.Single().Key);
        Assert.Equal("blue", saved.Options.Single().Value);
    }

    private static async Task<Guid> SeedProduct(AppDbContext context)
    {
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Tee",
            Sku = "TEE-1",
            Slug = "tee-1",
            Price = 100,
            Stock = 10,
            CategoryId = categoryId
        });
        await context.SaveChangesAsync();
        return productId;
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new FakeCurrentUserService());
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => Guid.NewGuid();
        public string? Email => "test@example.com";
        public string GetActor() => "test-user";
    }
}

