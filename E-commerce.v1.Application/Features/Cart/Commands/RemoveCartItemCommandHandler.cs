using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Unit>
{
    private readonly IAppDbContext _context;

    public RemoveCartItemCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new NotFoundException("Mục trong giỏ hàng không tồn tại.");

        if (cartItem.Cart?.UserId != request.UserId)
            throw new BadRequestException("Bạn không có quyền xóa mục này.");

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
