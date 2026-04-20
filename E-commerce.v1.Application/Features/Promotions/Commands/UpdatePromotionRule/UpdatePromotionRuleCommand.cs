using E_commerce.v1.Application.DTOs.Promotion;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;

public record UpdatePromotionRuleCommand(Guid Id, PromotionRuleUpsertDto Rule) : IRequest;

