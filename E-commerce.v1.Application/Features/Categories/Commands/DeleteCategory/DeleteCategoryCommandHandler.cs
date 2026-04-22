using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (category == null)
            throw new NotFoundException("Không tìm thấy danh mục cần xoá.");

        // Soft-delete để giữ lịch sử/quan hệ tham chiếu (không hard-delete).
        // Category sẽ bị ẩn khỏi query mặc định và không còn active để hiển thị storefront.
        category.IsDeleted = true;
        category.IsActive = false;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
