using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Features.Products.Services;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using Mapster;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductSlugService _slugService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IProductSlugService slugService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _slugService = slugService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _productRepository.GetCategoryByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

        var product = request.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.DocumentIds = request.DocumentIds?.ToList() ?? new List<string>();

        if (string.IsNullOrWhiteSpace(product.Sku))
            product.Sku = _slugService.GenerateSku(product.Id);

        if (string.IsNullOrWhiteSpace(product.Slug))
            product.Slug = _slugService.GenerateSlug(request.Name, product.Id);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
