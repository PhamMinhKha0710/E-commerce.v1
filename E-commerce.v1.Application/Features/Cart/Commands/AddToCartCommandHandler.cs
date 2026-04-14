using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly IAppDbContext _context;

    public AddToCartCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        if (!product.IsActive)
            throw new BadRequestException("Sản phẩm đã ngừng bán, không thể thêm vào giỏ.");

        if (product.Stock < request.Quantity)
            throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (attempt > 0)
                _context.ClearChangeTracker();

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

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (existingItem != null)
            {
                if (product.Stock < existingItem.Quantity + request.Quantity)
                    throw new BadRequestException("Số lượng cộng dồn vượt quá tồn kho.");

                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.CartItems.Add(new E_commerce.v1.Domain.Entities.CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            catch (DbUpdateException ex) when (attempt == 0 && IsDuplicateCartItemIndexViolation(ex))
            {
                // Hai request đồng thời thêm cùng (CartId, ProductId): merge ở lần thử sau.
            }
        }

        throw new InvalidOperationException("Không thể thêm vào giỏ sau khi xử lý trùng khóa.");
    }

    /// <summary>SQL Server: vi phạm unique index (2601/2627) hoặc tên index IX_CartItems.</summary>
    private static bool IsDuplicateCartItemIndexViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException;
        while (inner != null)
        {
            var typeName = inner.GetType().Name;
            if (typeName == "SqlException")
            {
                var number = inner.GetType().GetProperty("Number")?.GetValue(inner) as int?;
                if (number is 2601 or 2627)
                    return true;
            }

            var msg = inner.Message;
            if (msg.Contains("IX_CartItems", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                return true;

            inner = inner.InnerException;
        }

        return false;
    }
}
