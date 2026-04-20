using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;

public class CreateVariantCommandHandler : IRequestHandler<CreateVariantCommand, Guid>
{
    private readonly IVariantRepository _variantRepository;
    private readonly IProductQueryRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVariantCommandHandler(IVariantRepository variantRepository, IProductQueryRepository productRepository, IUnitOfWork unitOfWork)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        var normalizedSku = request.Sku.Trim();
        var skuExists = await _variantRepository.SkuExistsAsync(normalizedSku, excludeVariantId: null, cancellationToken);
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

        await _variantRepository.CreateVariantAsync(variant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }
}

