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
            .Include(r => r.Products)
            .Include(r => r.Categories)
            .Include(r => r.BuyXGetYAction)
            .Include(r => r.PercentageAction)
            .Where(r => r.IsActive && r.StartDate <= utcNow && r.EndDate >= utcNow)
            .ToListAsync(cancellationToken);
    }
}

