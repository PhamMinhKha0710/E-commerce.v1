using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Tests;

public class PromotionEngineTests
{
    [Fact]
    public async Task should_choose_highest_discount_when_multiple_rules_eligible()
    {
        await using var context = CreateContext();
        var product = await SeedProduct(context);

        var now = DateTime.UtcNow;
        context.PromotionRules.AddRange(
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                Name = "Promo 10%",
                Type = PromotionRuleType.PercentageDiscount,
                Priority = 1,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                IsActive = true,
                PercentageAction = new PromotionPercentageAction { Percent = 10, Target = PromotionDiscountTarget.EligibleItems }
            },
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                Name = "Promo 20%",
                Type = PromotionRuleType.PercentageDiscount,
                Priority = 0,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                IsActive = true,
                PercentageAction = new PromotionPercentageAction { Percent = 20, Target = PromotionDiscountTarget.EligibleItems }
            });
        await context.SaveChangesAsync();

        var items = new List<PromotionCartItem>
        {
            new(product.Id, product.CategoryId, UnitPrice: 100, Quantity: 1)
        };

        var readRepo = new PromotionRuleReadRepository(context);
        var best = await PromotionEngine.CalculateBestAsync(readRepo, items, now, CancellationToken.None);

        Assert.NotNull(best);
        Assert.Equal(20m, best!.DiscountAmount);
        Assert.Contains("20", best.Summary);
    }

    [Fact]
    public async Task should_tie_break_by_priority_when_discounts_equal()
    {
        await using var context = CreateContext();
        var product = await SeedProduct(context);

        var now = DateTime.UtcNow;
        var highPriority = new PromotionRule
        {
            Id = Guid.NewGuid(),
            Name = "High priority 10%",
            Type = PromotionRuleType.PercentageDiscount,
            Priority = 5,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            IsActive = true,
            PercentageAction = new PromotionPercentageAction { Percent = 10, Target = PromotionDiscountTarget.EligibleItems }
        };
        var lowPriority = new PromotionRule
        {
            Id = Guid.NewGuid(),
            Name = "Low priority 10%",
            Type = PromotionRuleType.PercentageDiscount,
            Priority = 1,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            IsActive = true,
            PercentageAction = new PromotionPercentageAction { Percent = 10, Target = PromotionDiscountTarget.EligibleItems }
        };
        context.PromotionRules.AddRange(highPriority, lowPriority);
        await context.SaveChangesAsync();

        var items = new List<PromotionCartItem>
        {
            new(product.Id, product.CategoryId, UnitPrice: 100, Quantity: 1)
        };

        var readRepo = new PromotionRuleReadRepository(context);
        var best = await PromotionEngine.CalculateBestAsync(readRepo, items, now, CancellationToken.None);

        Assert.NotNull(best);
        Assert.Equal(10m, best!.DiscountAmount);
        Assert.Equal(highPriority.Id, best.RuleId);
    }

    [Fact]
    public async Task should_apply_promotion_before_coupon_when_previewing_cart()
    {
        await using var context = CreateContext();
        var (userId, productId, categoryId) = await SeedCart(context);

        var now = DateTime.UtcNow;
        // 10% promotion => discount 10 on subtotal 100
        context.PromotionRules.Add(new PromotionRule
        {
            Id = Guid.NewGuid(),
            Name = "Promo 10%",
            Type = PromotionRuleType.PercentageDiscount,
            Priority = 1,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            IsActive = true,
            PercentageAction = new PromotionPercentageAction { Percent = 10, Target = PromotionDiscountTarget.EligibleItems }
        });

        // Coupon preview set to 50, but should be capped by amountAfterPromotion (=90)
        var cart = await context.Carts.SingleAsync(c => c.UserId == userId);
        cart.AppliedCouponCode = "SAVE";
        cart.CouponDiscountPreview = 95;
        await context.SaveChangesAsync();

        var items = new List<PromotionCartItem> { new(productId, categoryId, 100, 1) };
        var readRepo = new PromotionRuleReadRepository(context);
        var bestPromo = await PromotionEngine.CalculateBestAsync(readRepo, items, now, CancellationToken.None);
        var subtotalAfterPromo = 100 - (bestPromo?.DiscountAmount ?? 0);
        var couponDiscount = Math.Min(cart.CouponDiscountPreview, subtotalAfterPromo);

        Assert.Equal(10m, bestPromo!.DiscountAmount);
        Assert.Equal(90m, subtotalAfterPromo);
        Assert.Equal(90m, couponDiscount);
    }

    private static async Task<Product> SeedProduct(AppDbContext context)
    {
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test product",
            Sku = "P-1",
            Slug = "p-1",
            Price = 100,
            Stock = 10,
            CategoryId = categoryId
        };
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat" });
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    private static async Task<(Guid UserId, Guid ProductId, Guid CategoryId)> SeedCart(AppDbContext context)
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        context.Users.Add(new User { Id = userId, FirstName = "U", LastName = "1", Email = "u1@example.com", PasswordHash = "hash" });
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat" });
        context.Products.Add(new Product { Id = productId, Name = "P", Sku = "P-1", Slug = "p-1", Price = 100, Stock = 10, CategoryId = categoryId });
        context.Carts.Add(new Cart { Id = Guid.NewGuid(), UserId = userId });
        await context.SaveChangesAsync();
        context.CartItems.Add(new CartItem { Id = Guid.NewGuid(), CartId = context.Carts.Single(c => c.UserId == userId).Id, ProductId = productId, Quantity = 1 });
        await context.SaveChangesAsync();
        return (userId, productId, categoryId);
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

