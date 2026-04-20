namespace E_commerce.v1.Application.DTOs.Category;

public class CategoryListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public bool IsActive { get; set; }
    public IList<CategoryChildDto> Children { get; set; } = new List<CategoryChildDto>();
    public IList<CategoryProductSummaryDto> Products { get; set; } = new List<CategoryProductSummaryDto>();
}

public class CategoryChildDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class CategoryProductSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
