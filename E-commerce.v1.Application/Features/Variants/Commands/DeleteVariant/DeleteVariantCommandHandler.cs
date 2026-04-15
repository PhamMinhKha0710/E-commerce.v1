using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;

public class DeleteVariantCommandHandler : IRequestHandler<DeleteVariantCommand>
{
    private readonly IAppDbContext _dbContext;

    public DeleteVariantCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (variant == null)
            throw new NotFoundException("Biến thể không tồn tại.");

        variant.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

