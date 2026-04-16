using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        var previousStatus = order.Status;
        order.Status = request.Status;

        if (previousStatus != OrderStatus.Completed && request.Status == OrderStatus.Completed)
        {
            var user = await _userRepository.GetByIdAsync(order.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            var earnedPoints = LoyaltyPolicy.CalculateEarnedPoints(order.GrandTotal);
            user.LoyaltyPoints += earnedPoints;
            user.LoyaltyRank = LoyaltyPolicy.ResolveRank(user.LoyaltyPoints);
        }

        await _orderRepository.UpdateOrderStatusAsync(request.OrderId, (int)request.Status, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
