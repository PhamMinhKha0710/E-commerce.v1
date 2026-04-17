using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IPromotionRuleReadRepository
{
    Task<List<PromotionRule>> GetActiveRulesAsync(DateTime utcNow, CancellationToken cancellationToken);
}

