using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using Mapster;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<Category> _categoryRepo;

    public CreateProductCommandHandler(IGenericRepository<Product> productRepo, IGenericRepository<Category> categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

        var product = request.Adapt<Product>();
        product.Id = Guid.NewGuid();
        
        await _productRepo.AddAsync(product);
        
        return product.Id;
    }
}
