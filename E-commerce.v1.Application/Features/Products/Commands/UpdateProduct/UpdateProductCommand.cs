using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name, 
    string? Description, 
    decimal Price, 
    int Stock, 
    Guid CategoryId) : IRequest<bool>;
