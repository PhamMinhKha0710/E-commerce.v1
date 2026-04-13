using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IGenericRepository<Product> _repository;

    public DeleteProductCommandHandler(IGenericRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            throw new NotFoundException("Không tìm thấy sản phẩm cần xoá.");

        // Soft Delete
        product.IsDeleted = true;
        await _repository.UpdateAsync(product);

        return true;
    }
}
