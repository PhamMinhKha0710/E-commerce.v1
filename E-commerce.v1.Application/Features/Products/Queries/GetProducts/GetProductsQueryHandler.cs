using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IAppDbContext _context;

    public GetProductsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            query = query.Where(p => p.Category.Slug == request.CategorySlug);

        if (request.StoreId.HasValue)
            query = query.Where(p => p.StoreId == request.StoreId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (request.Kind.HasValue)
            query = query.Where(p => p.Kind == request.Kind.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        var nameFilter = !string.IsNullOrWhiteSpace(request.Name)
            ? request.Name
            : request.SearchTerm;
        if (!string.IsNullOrWhiteSpace(nameFilter))
            query = query.Where(p => p.Name.Contains(nameFilter));

        var page = request.ResolvedPage;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            Discount = p.Discount,
            IsActive = p.IsActive,
            ProductType = p.ProductType,
            Kind = p.Kind,
            DocumentIds = p.DocumentIds.ToList(),
            CategoryName = p.Category.Name
        }).ToList();

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
