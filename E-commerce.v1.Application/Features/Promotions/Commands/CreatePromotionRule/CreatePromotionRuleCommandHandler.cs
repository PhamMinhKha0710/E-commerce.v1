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

    public CreatePromotionRuleCommandHandler(
        IPromotionRuleRepository promotionRuleRepository,
        IPromotionRuleBuilderService promotionRuleBuilderService)
    {
        _promotionRuleRepository = promotionRuleRepository;
        _promotionRuleBuilderService = promotionRuleBuilderService;
    }

    public async Task<Guid> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Rule;

        // Create promotion rule entity
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

        // Apply scope and action
        await _promotionRuleBuilderService.ApplyScopeAsync(rule, dto.Scope, cancellationToken);
        _promotionRuleBuilderService.ApplyAction(rule, dto);

        // Persist rule
        await _promotionRuleRepository.AddAsync(rule, cancellationToken);
        await _promotionRuleRepository.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}

