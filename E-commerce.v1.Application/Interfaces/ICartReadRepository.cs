using E_commerce.v1.Application.DTOs.Cart;

namespace E_commerce.v1.Application.Interfaces;

public interface ICartReadRepository
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken);
}

