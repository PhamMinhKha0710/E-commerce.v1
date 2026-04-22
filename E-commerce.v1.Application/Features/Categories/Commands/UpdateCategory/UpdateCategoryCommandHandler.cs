using E_commerce.v1.Application.Features.Categories.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategorySlugService _slugService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ICategorySlugService slugService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _slugService = slugService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (category == null)
            throw new NotFoundException("Danh mục không tồn tại.");

        if (request.ParentCategoryId.HasValue)
        {
            if (request.ParentCategoryId.Value == category.Id)
                throw new BadRequestException("Danh mục không thể là cha của chính nó.");

            var parentExists = await _categoryRepository.ExistsByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new NotFoundException("Danh mục cha không tồn tại.");
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.Image = request.Image;
        category.ParentCategoryId = request.ParentCategoryId;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? _slugService.GenerateSlug(category.Name, category.Id)
            : request.Slug.Trim().ToLowerInvariant();

        if (!string.Equals(desiredSlug, category.Slug, StringComparison.Ordinal))
        {
            var slugTaken = await _categoryRepository.SlugExistsAsync(desiredSlug, excludeId: category.Id, cancellationToken);
            if (slugTaken)
                throw new BadRequestException("Slug đã tồn tại, vui lòng chọn giá trị khác.");
            category.Slug = desiredSlug;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
