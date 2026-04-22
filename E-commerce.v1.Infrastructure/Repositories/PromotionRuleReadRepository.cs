using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class PromotionRuleReadRepository : IPromotionRuleReadRepository
{
    private readonly AppDbContext _context;

    public PromotionRuleReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<PromotionRule>> GetActiveRulesAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return _context.PromotionRules
            .AsNoTracking()
            .AsSplitQuery()
            .Where(r => r.IsActive && r.StartDate <= utcNow && r.EndDate >= utcNow)
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<PromotionRuleDto>> SearchAsync(
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : pageSize;

        var query = _context.PromotionRules.AsNoTracking().AsQueryable();
        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(r => new PromotionRuleDto
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type,
                Priority = r.Priority,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PromotionRuleDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }

    public async Task<PromotionRuleDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await _context.PromotionRules
            .AsNoTracking()
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null) return null;

        return new PromotionRuleDetailDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Type = rule.Type,
            Priority = rule.Priority,
            StartDate = rule.StartDate,
            EndDate = rule.EndDate,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt,
            Scope = new PromotionRuleScopeDto
            {
                ProductIds = rule.Products.Select(p => p.ProductId).ToList(),
                CategoryIds = rule.Categories.Select(c => c.CategoryId).ToList()
            },
            BuyXGetYAction = rule.BuyXGetYAction == null ? null : new BuyXGetYActionDto
            {
                BuyProductId = rule.BuyXGetYAction.BuyProductId,
                BuyCategoryId = rule.BuyXGetYAction.BuyCategoryId,
                BuyQty = rule.BuyXGetYAction.BuyQty,
                GetProductId = rule.BuyXGetYAction.GetProductId,
                GetCategoryId = rule.BuyXGetYAction.GetCategoryId,
                GetQty = rule.BuyXGetYAction.GetQty,
                LimitPerOrder = rule.BuyXGetYAction.LimitPerOrder
            },
            PercentageAction = rule.PercentageAction == null ? null : new PercentageActionDto
            {
                Percent = rule.PercentageAction.Percent,
                Target = rule.PercentageAction.Target
            }
        };
    }
}
