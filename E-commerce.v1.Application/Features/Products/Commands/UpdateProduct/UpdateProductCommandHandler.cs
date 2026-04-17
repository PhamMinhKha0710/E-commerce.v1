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
        var product = await _productRepository.UpdateProductAsync(request.Id, p =>
        {
            p.Name = request.Name;
            p.Description = request.Description;
            p.Price = request.Price;
            p.Stock = request.Stock;
            p.CategoryId = request.CategoryId;
            p.NameEn = request.NameEn;
            p.DescriptionEn = request.DescriptionEn;
            if (!string.IsNullOrWhiteSpace(request.Sku))
                p.Sku = request.Sku;
            if (!string.IsNullOrWhiteSpace(request.Slug))
                p.Slug = request.Slug;
            if (request.Discount.HasValue)
                p.Discount = request.Discount.Value;
            p.Unit = request.Unit;
            p.UnitEn = request.UnitEn;
            if (request.IsActive.HasValue)
                p.IsActive = request.IsActive.Value;
            if (request.ProductType.HasValue)
                p.ProductType = request.ProductType.Value;
            if (request.Kind.HasValue)
                p.Kind = request.Kind.Value;
            if (request.BuyType.HasValue)
                p.BuyType = request.BuyType.Value;
            if (request.Priority.HasValue)
                p.Priority = request.Priority.Value;
            if (request.DocumentIds is not null)
                p.DocumentIds = request.DocumentIds.ToList();
            p.StoreId = request.StoreId;
        }, cancellationToken);

        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
