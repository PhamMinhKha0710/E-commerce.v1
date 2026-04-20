using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Features.Cart.Queries;
using E_commerce.v1.Application.Interfaces;
using CartEntity = E_commerce.v1.Domain.Entities.Cart;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class SyncCartCommandHandler : IRequestHandler<SyncCartCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public SyncCartCommandHandler(
        ICartRepository cartRepository,
        IMediator mediator,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> Handle(SyncCartCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _cartRepository.UserExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
            throw new BadRequestException("Tài khoản không tồn tại trong hệ thống. Vui lòng đăng nhập lại.");

        var guestByProduct = request.Items
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (guestByProduct.Count == 0)
            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);

        var cart = await _cartRepository.GetCartWithItemsAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new CartEntity
            {
                UserId = request.UserId,
                CartItems = new List<CartItem>()
            };
        }

        cart.CartItems ??= new List<CartItem>();

        var initialDbQty = cart.CartItems
            .GroupBy(ci => ci.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var (productId, guestQty) in guestByProduct)
        {
            var product = await _cartRepository.GetProductByIdAsync(productId, cancellationToken);

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
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = merged
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
    }
}
