using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Common.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, IUnitOfWork unitOfWork, ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddToCart requested. UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}", userId, productId, quantity);
        var product = await _cartRepository.GetProductByIdAsync(productId, cancellationToken);
        if (product == null) throw new NotFoundException("Sản phẩm không tồn tại.");
        if (!product.IsActive) throw new BadRequestException("Sản phẩm đã ngừng bán, không thể thêm vào giỏ.");
        if (product.Stock < quantity) throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        var userExists = await _cartRepository.UserExistsAsync(userId, cancellationToken);
        if (!userExists) throw new BadRequestException("Tài khoản không tồn tại trong hệ thống. Vui lòng đăng nhập lại.");

        var cart = await _cartRepository.GetCartWithItemsAsync(userId, cancellationToken);
        var existingQty = cart?.CartItems.Where(ci => ci.ProductId == productId).Sum(ci => ci.Quantity) ?? 0;
        if (product.Stock < existingQty + quantity)
            throw new BadRequestException("Số lượng cộng dồn vượt quá tồn kho.");

        await _cartRepository.AddToCartAsync(userId, productId, quantity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("AddToCart completed. UserId={UserId}, ProductId={ProductId}", userId, productId);
    }
}

