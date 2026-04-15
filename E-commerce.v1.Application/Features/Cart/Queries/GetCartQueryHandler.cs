using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IAppDbContext _context;

    public GetCartQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
            return EmptyCart(request.UserId);

        return await MapCartAsync(_context, cart, cancellationToken);
    }

    /// <summary>Chưa có bản ghi Cart trong DB — trả về giỏ logic rỗng (Id = Empty để phân biệt).</summary>
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

    private static async Task<CartDto> MapCartAsync(
        IAppDbContext context,
        E_commerce.v1.Domain.Entities.Cart cart,
        CancellationToken cancellationToken)
    {
        var items = cart.CartItems.Select(ci =>
        {
            var unit = ci.Product?.Price ?? 0;
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
        var bestPromo = await PromotionEngine.CalculateBestAsync(context, promoItems, now, cancellationToken);
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
}
