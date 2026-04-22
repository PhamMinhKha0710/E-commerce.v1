using E_commerce.v1.Application.Common;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateOrderStatus requested. OrderId={OrderId}, NewStatus={NewStatus}", request.OrderId, request.Status);
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

        await _orderRepository.UpdateOrderStatusAsync(request.OrderId, request.Status, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order status updated. OrderId={OrderId}, PreviousStatus={PreviousStatus}, NewStatus={NewStatus}", request.OrderId, previousStatus, request.Status);
        return Unit.Value;
    }
}
