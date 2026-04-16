using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class PromotionRuleRepository : IPromotionRuleRepository
{
    private readonly AppDbContext _context;

    public PromotionRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PromotionRule> AddAsync(PromotionRule rule, CancellationToken cancellationToken)
    {
        await _context.PromotionRules.AddAsync(rule, cancellationToken);
        return rule;
    }

    public Task<PromotionRule?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.PromotionRules
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<bool> DeleteAsync(PromotionRule rule, CancellationToken cancellationToken)
    {
        _context.PromotionRules.Remove(rule);
        return Task.FromResult(true);
    }
}
