using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Features.Promotions.Services;

/// <summary>
/// Service for building and configuring promotion rules
/// </summary>
public interface IPromotionRuleBuilderService
{
    /// <summary>
    /// Apply scope (products, categories) to promotion rule
    /// </summary>
    Task ApplyScopeAsync(PromotionRule rule, PromotionRuleScopeDto? scope, CancellationToken cancellationToken);

    /// <summary>
    /// Apply discount action to promotion rule
    /// </summary>
    void ApplyAction(PromotionRule rule, PromotionRuleUpsertDto dto);
}
