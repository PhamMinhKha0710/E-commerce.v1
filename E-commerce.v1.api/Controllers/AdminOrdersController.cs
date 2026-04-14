using E_commerce.v1.Application.Features.Order.Commands.UpdateOrderStatus;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/admin/orders")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
            return BadRequest(new { Message = "Trạng thái đơn hàng không hợp lệ." });

        await _mediator.Send(new UpdateOrderStatusCommand(id, (OrderStatus)request.Status));
        return NoContent();
    }
}

public class UpdateOrderStatusRequest
{
    public int Status { get; set; }
}
