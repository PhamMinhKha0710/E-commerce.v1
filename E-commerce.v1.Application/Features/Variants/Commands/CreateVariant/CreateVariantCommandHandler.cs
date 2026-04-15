using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;

public class CreateVariantCommandHandler : IRequestHandler<CreateVariantCommand, Guid>
{
    private readonly IAppDbContext _dbContext;

    public CreateVariantCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        var normalizedSku = request.Sku.Trim();
        var skuExists = await _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(v => v.Sku == normalizedSku, cancellationToken);
        if (skuExists)
            throw new BadRequestException("SKU đã tồn tại trong hệ thống.");

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Sku = normalizedSku,
            Price = request.Price,
            Inventory = request.Inventory,
            IsActive = request.IsActive
        };

        if (request.Options != null)
        {
            foreach (var opt in request.Options)
            {
                var key = opt.Key.Trim().ToLowerInvariant();
                var value = opt.Value.Trim();
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                variant.Options.Add(new ProductVariantOption
                {
                    VariantId = variant.Id,
                    Key = key,
                    Value = value
                });
            }
        }

        await _dbContext.ProductVariants.AddAsync(variant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }
}

