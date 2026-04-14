using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateCartItemCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy CartItem
        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new NotFoundException("Mục trong giỏ hàng không tồn tại.");

        // 2. Kiểm tra xem CartItem có thuộc về user không
        if (cartItem.Cart?.UserId != request.UserId)
            throw new BadRequestException("Bạn không có quyền cập nhật mục này.");

        // 3. Xóa dòng — luôn cho phép (kể cả SP ngừng bán / không load được)
        if (request.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        var product = cartItem.Product;
        if (product == null)
            throw new NotFoundException("Sản phẩm không còn tồn tại.");

        if (!product.IsActive)
            throw new BadRequestException("Sản phẩm đã ngừng bán. Chỉ có thể giảm số lượng về 0 để xóa khỏi giỏ.");

        if (product.Stock < request.Quantity)
            throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        cartItem.Quantity = request.Quantity;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
