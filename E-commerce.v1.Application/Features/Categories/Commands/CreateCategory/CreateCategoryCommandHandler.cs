using E_commerce.v1.Application.Features.Categories.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategorySlugService _slugService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ICategorySlugService slugService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _slugService = slugService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new NotFoundException("Danh mục cha không tồn tại.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Image = request.Image,
            IsActive = request.IsActive,
            ParentCategoryId = request.ParentCategoryId
        };

        category.Slug = string.IsNullOrWhiteSpace(request.Slug)
            ? _slugService.GenerateSlug(category.Name, category.Id)
            : request.Slug.Trim().ToLowerInvariant();

        var slugTaken = await _categoryRepository.SlugExistsAsync(category.Slug, excludeId: null, cancellationToken);
        if (slugTaken)
            throw new BadRequestException("Slug đã tồn tại, vui lòng chọn giá trị khác.");

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
