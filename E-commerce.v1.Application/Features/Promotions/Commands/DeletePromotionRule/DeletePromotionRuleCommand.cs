using MediatR;

namespace E_commerce.v1.Application.Features.Promotions.Commands.DeletePromotionRule;

public record DeletePromotionRuleCommand(Guid Id) : IRequest;

