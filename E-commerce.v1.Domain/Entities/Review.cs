using E_commerce.v1.Domain.Common;

namespace E_commerce.v1.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Rating { get; set; }
    public string? Comment { get; set; }
}
