using E_commerce.v1.Application.DTOs.Promotion;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;

public record CreatePromotionRuleCommand(PromotionRuleUpsertDto Rule) : IRequest<Guid>;

