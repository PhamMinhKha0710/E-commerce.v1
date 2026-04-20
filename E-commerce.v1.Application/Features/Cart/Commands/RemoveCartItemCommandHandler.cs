using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Unit>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCartItemCommandHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _cartRepository.GetCartItemWithDetailsAsync(request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new NotFoundException("Mục trong giỏ hàng không tồn tại.");

        if (cartItem.Cart?.UserId != request.UserId)
            throw new BadRequestException("Bạn không có quyền xóa mục này.");

        await _cartRepository.RemoveCartItemAsync(request.CartItemId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
