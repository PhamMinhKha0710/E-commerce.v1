using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Mapster;

namespace E_commerce.v1.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IAppDbContext _context;

    public GetProductByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .ThenInclude(v => v.Options)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            
        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        var dto = product.Adapt<ProductDetailDto>();
        dto.CategoryName = product.Category.Name;

        dto.Variants = product.Variants
            .Where(v => v.IsActive)
            .Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Price = v.Price,
                Inventory = v.Inventory,
                IsActive = v.IsActive,
                Options = v.Options
                    .Select(o => new ProductVariantOptionDto { Key = o.Key, Value = o.Value })
                    .ToList()
            })
            .ToList();
        
        return dto;
    }
}
