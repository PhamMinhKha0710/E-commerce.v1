using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly IAppDbContext _dbContext;

    public UpdateProductCommandHandler(
        IGenericRepository<Product> productRepo,
        IGenericRepository<Category> categoryRepo,
        IAppDbContext dbContext)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepo.GetByIdAsync(request.Id);
        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

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

        await _productRepo.UpdateAsync(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
