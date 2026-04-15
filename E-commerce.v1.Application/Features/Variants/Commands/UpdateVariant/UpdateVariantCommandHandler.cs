using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandHandler : IRequestHandler<UpdateVariantCommand>
{
    private readonly IAppDbContext _dbContext;

    public UpdateVariantCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _dbContext.ProductVariants
            .Include(v => v.Options)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (variant == null)
            throw new NotFoundException("Biến thể không tồn tại.");

        var normalizedSku = request.Sku.Trim();
        var skuExists = await _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(v => v.Sku == normalizedSku && v.Id != request.Id, cancellationToken);
        if (skuExists)
            throw new BadRequestException("SKU đã tồn tại trong hệ thống.");

        variant.Sku = normalizedSku;
        variant.Price = request.Price;
        variant.Inventory = request.Inventory;
        variant.IsActive = request.IsActive;

        // Replace options (simple + deterministic)
        variant.Options.Clear();
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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

