using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.DeletePromotionRule;

public class DeletePromotionRuleCommandHandler : IRequestHandler<DeletePromotionRuleCommand>
{
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePromotionRuleCommandHandler(IPromotionRuleRepository promotionRuleRepository, IUnitOfWork unitOfWork)
    {
        _promotionRuleRepository = promotionRuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _promotionRuleRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (rule == null)
            throw new NotFoundException("Promotion rule không tồn tại.");

        await _promotionRuleRepository.DeleteAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

