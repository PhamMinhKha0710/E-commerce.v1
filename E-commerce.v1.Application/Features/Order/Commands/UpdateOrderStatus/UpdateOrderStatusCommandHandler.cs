using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateOrderStatusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        var previousStatus = order.Status;
        order.Status = request.Status;

        if (previousStatus != OrderStatus.Completed && request.Status == OrderStatus.Completed)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            var earnedPoints = LoyaltyPolicy.CalculateEarnedPoints(order.GrandTotal);
            user.LoyaltyPoints += earnedPoints;
            user.LoyaltyRank = LoyaltyPolicy.ResolveRank(user.LoyaltyPoints);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
