using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRules;

public record GetPromotionRulesQuery(
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<PromotionRuleDto>>;
