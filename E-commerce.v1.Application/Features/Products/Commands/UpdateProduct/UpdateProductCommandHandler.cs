using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductQueryRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductQueryRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductForUpdateAsync(request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.NameEn = request.NameEn;
        product.DescriptionEn = request.DescriptionEn;
        if (!string.IsNullOrWhiteSpace(request.Sku))
            product.Sku = request.Sku;
        if (!string.IsNullOrWhiteSpace(request.Slug))
            product.Slug = request.Slug;
        if (request.Discount.HasValue)
            product.Discount = request.Discount.Value;
        product.Unit = request.Unit;
        product.UnitEn = request.UnitEn;
        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;
        if (request.ProductType.HasValue)
            product.ProductType = request.ProductType.Value;
        if (request.Kind.HasValue)
            product.Kind = request.Kind.Value;
        if (request.BuyType.HasValue)
            product.BuyType = request.BuyType.Value;
        if (request.Priority.HasValue)
            product.Priority = request.Priority.Value;
        if (request.DocumentIds is not null)
            product.DocumentIds = request.DocumentIds.ToList();
        product.StoreId = request.StoreId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
