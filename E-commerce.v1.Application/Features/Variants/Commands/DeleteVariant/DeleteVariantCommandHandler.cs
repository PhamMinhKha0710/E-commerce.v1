using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;

public class DeleteVariantCommandHandler : IRequestHandler<DeleteVariantCommand>
{
    private readonly IVariantRepository _variantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVariantCommandHandler(IVariantRepository variantRepository, IUnitOfWork unitOfWork)
    {
        _variantRepository = variantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _variantRepository.SoftDeleteVariantAsync(request.Id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Biến thể không tồn tại.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

