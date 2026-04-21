using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace E_commerce.v1.Infrastructure.Repositories;

/// <summary>
/// Decorator caching active promotion rules to reduce DB load on hot paths
/// (e.g. GET /api/v1/cart). TTL is intentionally short so rule changes
/// propagate within ~1 minute without requiring explicit invalidation.
/// </summary>
public class CachedPromotionRuleReadRepository : IPromotionRuleReadRepository
{
    private const string ActiveRulesCacheKey = "promotion_rules:active";
    private static readonly TimeSpan ActiveRulesTtl = TimeSpan.FromSeconds(60);

    private readonly PromotionRuleReadRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedPromotionRuleReadRepository(PromotionRuleReadRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<PromotionRule>> GetActiveRulesAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<List<PromotionRule>>(ActiveRulesCacheKey, out var cached) && cached != null)
        {
            return FilterByTime(cached, utcNow);
        }

        // Load a superset (all currently active) and cache it. Time-window
        // filtering is applied per call so a single cache entry serves slightly
        // different utcNow values within the TTL window.
        var rules = await _inner.GetActiveRulesAsync(utcNow, cancellationToken);

        _cache.Set(ActiveRulesCacheKey, rules, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ActiveRulesTtl
        });

        return FilterByTime(rules, utcNow);
    }

    public Task<PagedResult<PromotionRuleDto>> SearchAsync(
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
        => _inner.SearchAsync(isActive, page, pageSize, cancellationToken);

    public Task<PromotionRuleDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken)
        => _inner.GetDetailByIdAsync(id, cancellationToken);

    private static List<PromotionRule> FilterByTime(List<PromotionRule> rules, DateTime utcNow)
    {
        return rules
            .Where(r => r.IsActive && r.StartDate <= utcNow && r.EndDate >= utcNow)
            .ToList();
    }
}
