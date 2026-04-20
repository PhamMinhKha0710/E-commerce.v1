using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Features.Promotions.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;

public class CreatePromotionRuleCommandHandler : IRequestHandler<CreatePromotionRuleCommand, Guid>
{
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly IPromotionRuleBuilderService _promotionRuleBuilderService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePromotionRuleCommandHandler(
        IPromotionRuleRepository promotionRuleRepository,
        IPromotionRuleBuilderService promotionRuleBuilderService,
        IUnitOfWork unitOfWork)
    {
        _promotionRuleRepository = promotionRuleRepository;
        _promotionRuleBuilderService = promotionRuleBuilderService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Rule;

        var rule = new PromotionRule
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Type = dto.Type,
            Priority = dto.Priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };

        await _promotionRuleBuilderService.ApplyScopeAsync(rule, dto.Scope, cancellationToken);
        _promotionRuleBuilderService.ApplyAction(rule, dto);

        await _promotionRuleRepository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}

