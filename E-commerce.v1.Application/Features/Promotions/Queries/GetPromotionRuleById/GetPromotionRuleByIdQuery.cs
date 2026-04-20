using E_commerce.v1.Application.DTOs.Promotion;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRuleById;

public record GetPromotionRuleByIdQuery(Guid Id) : IRequest<PromotionRuleDetailDto>;
