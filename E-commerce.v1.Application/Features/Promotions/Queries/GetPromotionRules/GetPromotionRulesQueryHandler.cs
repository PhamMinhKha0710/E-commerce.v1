using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRules;

public class GetPromotionRulesQueryHandler : IRequestHandler<GetPromotionRulesQuery, PagedResult<PromotionRuleDto>>
{
    private readonly IPromotionRuleReadRepository _repository;

    public GetPromotionRulesQueryHandler(IPromotionRuleReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<PromotionRuleDto>> Handle(GetPromotionRulesQuery request, CancellationToken cancellationToken)
    {
        return _repository.SearchAsync(request.IsActive, request.PageNumber, request.PageSize, cancellationToken);
    }
}
