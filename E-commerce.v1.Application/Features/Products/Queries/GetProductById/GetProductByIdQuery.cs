using E_commerce.v1.Application.DTOs.Product;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto>;
