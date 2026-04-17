using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRuleById;

public class GetPromotionRuleByIdQueryHandler : IRequestHandler<GetPromotionRuleByIdQuery, PromotionRuleDetailDto>
{
    private readonly IPromotionRuleReadRepository _repository;

    public GetPromotionRuleByIdQueryHandler(IPromotionRuleReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PromotionRuleDetailDto> Handle(GetPromotionRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetDetailByIdAsync(request.Id, cancellationToken)
                  ?? throw new NotFoundException("Không tìm thấy promotion rule.");
        return dto;
    }
}
