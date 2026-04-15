using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;

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

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
