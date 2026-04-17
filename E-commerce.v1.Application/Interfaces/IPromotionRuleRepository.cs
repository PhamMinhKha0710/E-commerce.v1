using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IPromotionRuleRepository
{
    Task<PromotionRule> AddAsync(PromotionRule rule, CancellationToken cancellationToken);

    Task<PromotionRule?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(PromotionRule rule, CancellationToken cancellationToken);
}
