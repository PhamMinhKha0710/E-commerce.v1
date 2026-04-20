using System.Data;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using MediatR;

namespace E_commerce.v1.Application.Features.Order.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Unit>
{
    private static readonly HashSet<OrderStatus> CancellableStatuses = new()
    {
        OrderStatus.Pending,
        OrderStatus.Confirmed
    };

    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await _orderRepository.GetOrderByIdAsync(request.OrderId, ct)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.UserId != request.UserId)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.Status == OrderStatus.Cancelled)
                return;

            if (!CancellableStatuses.Contains(order.Status))
                throw new BadRequestException("Đơn hàng ở trạng thái hiện tại không thể huỷ.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new BadRequestException("Đơn hàng đã thanh toán, vui lòng sử dụng luồng hoàn tiền.");

            order.Status = OrderStatus.Cancelled;
            await _paymentRepository.ReleaseReservedStockAsync(order.Id, DateTime.UtcNow, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, IsolationLevel.ReadCommitted, cancellationToken);

        return Unit.Value;
    }
}
