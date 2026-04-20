using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Domain.Entities;

namespace E_commerce.v1.Application.Interfaces;

public interface IPromotionRuleReadRepository
{
    Task<List<PromotionRule>> GetActiveRulesAsync(DateTime utcNow, CancellationToken cancellationToken);

    Task<PagedResult<PromotionRuleDto>> SearchAsync(
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PromotionRuleDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken);
}

