using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Unit>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemCommandHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _cartRepository.GetCartItemWithDetailsAsync(request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new NotFoundException("Mục trong giỏ hàng không tồn tại.");

        if (cartItem.Cart?.UserId != request.UserId)
            throw new BadRequestException("Bạn không có quyền cập nhật mục này.");

        if (request.Quantity <= 0)
        {
            await _cartRepository.RemoveCartItemAsync(request.CartItemId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        var product = cartItem.Product;
        if (product == null)
            throw new NotFoundException("Sản phẩm không còn tồn tại.");

        if (!product.IsActive)
            throw new BadRequestException("Sản phẩm đã ngừng bán. Chỉ có thể giảm số lượng về 0 để xóa khỏi giỏ.");

        if (product.Stock < request.Quantity)
            throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        await _cartRepository.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
