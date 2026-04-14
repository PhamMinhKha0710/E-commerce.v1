using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name, 
    string? Description, 
    decimal Price, 
    int Stock, 
    Guid CategoryId) : IRequest<Guid>;
