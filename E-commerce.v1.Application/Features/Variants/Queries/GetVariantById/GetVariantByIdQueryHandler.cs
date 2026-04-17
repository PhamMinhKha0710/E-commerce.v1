using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Queries.GetVariantById;

public class GetVariantByIdQueryHandler : IRequestHandler<GetVariantByIdQuery, ProductVariantDto>
{
    private readonly IVariantReadRepository _repository;

    public GetVariantByIdQueryHandler(IVariantReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductVariantDto> Handle(GetVariantByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new NotFoundException("Không tìm thấy variant.");
        return dto;
    }
}
