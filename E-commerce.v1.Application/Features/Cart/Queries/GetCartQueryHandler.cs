using E_commerce.v1.Application.DTOs.Cart;
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

        return MapCart(cart);
    }

    /// <summary>Chưa có bản ghi Cart trong DB — trả về giỏ logic rỗng (Id = Empty để phân biệt).</summary>
    private static CartDto EmptyCart(Guid userId) => new()
    {
        Id = Guid.Empty,
        UserId = userId,
        CartItems = new List<CartItemDto>(),
        TotalAmount = 0
    };

    private static CartDto MapCart(E_commerce.v1.Domain.Entities.Cart cart)
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

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItems = items,
            TotalAmount = items.Sum(i => i.TotalPrice)
        };
    }
}
