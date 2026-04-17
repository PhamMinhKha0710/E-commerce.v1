using E_commerce.v1.Application.DTOs.Product;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Queries.GetVariantById;

public record GetVariantByIdQuery(Guid Id) : IRequest<ProductVariantDto>;
