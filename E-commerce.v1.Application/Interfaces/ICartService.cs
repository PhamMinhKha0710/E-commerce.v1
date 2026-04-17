namespace E_commerce.v1.Application.Interfaces;

public interface ICartService
{
    Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken);
}

