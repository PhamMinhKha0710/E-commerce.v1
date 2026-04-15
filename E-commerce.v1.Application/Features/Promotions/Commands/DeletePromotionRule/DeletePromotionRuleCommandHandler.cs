using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Promotions.Commands.DeletePromotionRule;

public class DeletePromotionRuleCommandHandler : IRequestHandler<DeletePromotionRuleCommand>
{
    private readonly IAppDbContext _context;

    public DeletePromotionRuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.PromotionRules.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rule == null)
            throw new NotFoundException("Promotion rule không tồn tại.");

        _context.PromotionRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

