using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CartReadRepository : ICartReadRepository
{
    private readonly AppDbContext _context;
    private readonly IPromotionRuleReadRepository _promotionRuleReadRepository;

    public CartReadRepository(AppDbContext context, IPromotionRuleReadRepository promotionRuleReadRepository)
    {
        _context = context;
        _promotionRuleReadRepository = promotionRuleReadRepository;
    }

    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
            return EmptyCart(userId);

        var items = cart.CartItems.Select(ci =>
        {
            var unit = ci.Product?.Price ?? 0m;
            return new CartItemDto
            {
                Id = ci.Id,
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? string.Empty,
                ProductPrice = unit,
                Quantity = ci.Quantity,
                TotalPrice = unit * ci.Quantity
            };
        }).ToList();

        var promoItems = cart.CartItems
            .Where(ci => ci.Product != null)
            .Select(ci => new PromotionCartItem(
                ci.ProductId,
                ci.Product!.CategoryId,
                ci.Product.Price,
                ci.Quantity))
            .ToList();

        var now = DateTime.UtcNow;
        var bestPromo = await PromotionEngine.CalculateBestAsync(_promotionRuleReadRepository, promoItems, now, cancellationToken);

        var promotionDiscount = bestPromo?.DiscountAmount ?? 0m;
        var subtotalAfterPromotion = Math.Max(0, items.Sum(i => i.TotalPrice) - promotionDiscount);
        var couponDiscount = Math.Min(cart.CouponDiscountPreview, subtotalAfterPromotion);

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItems = items,
            TotalAmount = items.Sum(i => i.TotalPrice),
            AppliedPromotionRuleId = bestPromo?.RuleId,
            PromotionSummary = bestPromo?.Summary,
            PromotionDiscount = promotionDiscount,
            AppliedCouponCode = cart.AppliedCouponCode,
            CouponDiscount = couponDiscount,
            FinalAmount = Math.Max(0, subtotalAfterPromotion - couponDiscount)
        };
    }

    private static CartDto EmptyCart(Guid userId) => new()
    {
        Id = Guid.Empty,
        UserId = userId,
        CartItems = new List<CartItemDto>(),
        TotalAmount = 0,
        PromotionDiscount = 0,
        CouponDiscount = 0,
        FinalAmount = 0
    };
}

