using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class ProductQueryRepository : IProductQueryRepository
{
    private readonly AppDbContext _context;

    public ProductQueryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Product> Items, int Total)> GetProductsAsync(
        int skip,
        int take,
        string? categoryId,
        string? search,
        string? sortBy,
        CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId))
            query = query.Where(p => p.CategoryId == catId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description!.Contains(search));

        var total = await query.CountAsync(cancellationToken);

        query = query.Skip(skip).Take(take);

        var items = await query.ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetProductForUpdateAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
            return true;
        }
        return false;
    }
}
