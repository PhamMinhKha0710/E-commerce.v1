using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Features.Cart.Queries;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class SyncCartCommandHandler : IRequestHandler<SyncCartCommand, CartDto>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public SyncCartCommandHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto> Handle(SyncCartCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            throw new BadRequestException("Tài khoản không tồn tại trong hệ thống. Vui lòng đăng nhập lại.");

        // Gộp trùng ProductId từ localStorage (nhiều dòng cùng SP → cộng quantity).
        var guestByProduct = request.Items
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (guestByProduct.Count == 0)
            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new E_commerce.v1.Domain.Entities.Cart
            {
                UserId = request.UserId,
                CartItems = new List<E_commerce.v1.Domain.Entities.CartItem>()
            };
            _context.Carts.Add(cart);
        }

        cart.CartItems ??= new List<E_commerce.v1.Domain.Entities.CartItem>();

        var initialDbQty = cart.CartItems
            .GroupBy(ci => ci.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        // Validate toàn bộ từ DB: tồn tại, đang bán, đủ kho sau merge (không tin giá từ FE).
        foreach (var (productId, guestQty) in guestByProduct)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
                throw new NotFoundException($"Sản phẩm không tồn tại: {productId}.");

            if (!product.IsActive)
                throw new BadRequestException($"Sản phẩm '{product.Name}' đã ngừng bán, không thể đồng bộ.");

            var merged = initialDbQty.GetValueOrDefault(productId, 0) + guestQty;
            if (merged > product.Stock)
                throw new BadRequestException(
                    $"Sản phẩm '{product.Name}': sau khi gộp giỏ cần {merged} nhưng kho chỉ còn {product.Stock}.");
        }

        foreach (var (productId, guestQty) in guestByProduct)
        {
            var merged = initialDbQty.GetValueOrDefault(productId, 0) + guestQty;
            var existing = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existing != null)
                existing.Quantity = merged;
            else
            {
                cart.CartItems.Add(new E_commerce.v1.Domain.Entities.CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = merged
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
    }
}
