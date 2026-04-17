using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Features.Promotions.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;

public class UpdatePromotionRuleCommandHandler : IRequestHandler<UpdatePromotionRuleCommand>
{
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly IPromotionRuleBuilderService _promotionRuleBuilder;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePromotionRuleCommandHandler(
        IPromotionRuleRepository promotionRuleRepository,
        IPromotionRuleBuilderService promotionRuleBuilder,
        IUnitOfWork unitOfWork)
    {
        _promotionRuleRepository = promotionRuleRepository;
        _promotionRuleBuilder = promotionRuleBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _promotionRuleRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);

        if (rule == null)
            throw new NotFoundException("Promotion rule không tồn tại.");

        var dto = request.Rule;
        rule.Name = dto.Name.Trim();
        rule.Type = dto.Type;
        rule.Priority = dto.Priority;
        rule.StartDate = dto.StartDate;
        rule.EndDate = dto.EndDate;
        rule.IsActive = dto.IsActive;

        rule.Products.Clear();
        rule.Categories.Clear();
        await _promotionRuleBuilder.ApplyScopeAsync(rule, dto.Scope, cancellationToken);

        rule.BuyXGetYAction = null;
        rule.PercentageAction = null;
        _promotionRuleBuilder.ApplyAction(rule, dto);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

