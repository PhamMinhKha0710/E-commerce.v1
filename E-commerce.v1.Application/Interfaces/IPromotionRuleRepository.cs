using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

/// <summary>
/// Repository for PromotionRule entity operations
/// </summary>
public interface IPromotionRuleRepository
{
    /// <summary>
    /// Add promotion rule to database
    /// </summary>
    Task<PromotionRule> AddAsync(PromotionRule rule, CancellationToken cancellationToken);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
