namespace E_commerce.v1.Application.Features.Categories.Services;

/// <summary>
/// Sinh slug URL-friendly cho category (có kèm suffix ngắn từ Guid để tránh trùng).
/// </summary>
public interface ICategorySlugService
{
    string GenerateSlug(string categoryName, Guid categoryId);
}
