using E_commerce.v1.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ClearCartCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
            return Unit.Value;

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
