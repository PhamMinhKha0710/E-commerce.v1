using System.Globalization;
using System.Text;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IAppDbContext _dbContext;

    public CreateProductCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

        var product = request.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.DocumentIds = request.DocumentIds?.ToList() ?? new List<string>();

        if (string.IsNullOrWhiteSpace(product.Sku))
            product.Sku = $"SKU-{product.Id.ToString("N", CultureInfo.InvariantCulture)[..12].ToUpperInvariant()}";

        if (string.IsNullOrWhiteSpace(product.Slug))
            product.Slug = Slugify(request.Name) + "-" + product.Id.ToString("N", CultureInfo.InvariantCulture)[..8];

        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }

    private static string Slugify(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name.Normalize(NormalizationForm.FormD))
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(ch))
                sb.Append(char.ToLowerInvariant(ch));
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_')
                sb.Append('-');
        }
        var s = sb.ToString().Trim('-');
        return string.IsNullOrEmpty(s) ? "sp" : s;
    }
}
