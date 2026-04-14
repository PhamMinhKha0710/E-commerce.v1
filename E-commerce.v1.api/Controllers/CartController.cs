using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Features.Cart.Commands;
using E_commerce.v1.Application.Features.Cart.Queries;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/cart")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }


    /// Lấy giỏ hàng của user hiện tại

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserIdFromToken();
        var query = new GetCartQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Đồng bộ giỏ guest (localStorage) vào DB sau khi đăng nhập.
    /// Merge: mỗi sản phẩm quantity = số lượng trên DB + số lượng từ FE (không dùng giá từ client — giá lấy khi GET cart/checkout).
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<CartDto>> SyncCart([FromBody] SyncCartRequest request)
    {
        var userId = GetUserIdFromToken();
        var result = await _mediator.Send(new SyncCartCommand(userId, request.Items));
        return Ok(result);
    }

    /// Thêm sản phẩm vào giỏ hàng

    /// Kiểm tra sản phẩm có tồn tại và đảm bảo Inventory còn đủ hàng trong kho trước khi thêm vào giỏ hàng

    [HttpPost("add")]
    public async Task<ActionResult<Unit>> AddToCart([FromBody] AddToCartCommandRequest request)
    {
        var userId = GetUserIdFromToken();
        var command = new AddToCartCommand
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        await _mediator.Send(command);
        return Ok(new { Message = "Sản phẩm đã được thêm vào giỏ hàng thành công." });
    }


    /// Cập nhật số lượng sản phẩm trong giỏ hàng

    /// Nếu Quantity = 0, sản phẩm sẽ được xóa khỏi giỏ hàng
    /// Lấy ID người dùng từ Token JWT để truy xuất đúng giỏ hàng tương ứng
    /// 
    [HttpPut("items/{id}")]
    public async Task<ActionResult<Unit>> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        var userId = GetUserIdFromToken();
        var command = new UpdateCartItemCommand
        {
            UserId = userId,
            CartItemId = id,
            Quantity = request.Quantity
        };

        await _mediator.Send(command);
        return Ok(new { Message = "Giỏ hàng đã được cập nhật thành công." });
    }

    /// Xóa một dòng trong giỏ (RESTful, tương đương PUT quantity = 0).
    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveCartItem(Guid id)
    {
        var userId = GetUserIdFromToken();
        await _mediator.Send(new RemoveCartItemCommand(userId, id));
        return NoContent();
    }

    /// Xóa toàn bộ giỏ (xóa bản ghi Cart; lần GET sau trả giỏ rỗng).
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserIdFromToken();
        await _mediator.Send(new ClearCartCommand(userId));
        return NoContent();
    }

    /// Checkout giỏ hàng hiện tại thành đơn hàng.
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request)
    {
        var userId = GetUserIdFromToken();
        var result = await _mediator.Send(new CheckoutCommand(userId, request.PaymentMethod));
        return CreatedAtAction(nameof(Checkout), new { id = result.OrderId }, result);
    }

    /// Checkout các item được chọn trong giỏ hàng thành 1 đơn hàng.
    [HttpPost("checkout-selected")]
    public async Task<ActionResult<CheckoutResponse>> CheckoutSelected([FromBody] CheckoutSelectedRequest request)
    {
        var userId = GetUserIdFromToken();
        var result = await _mediator.Send(new CheckoutSelectedCommand(userId, request.CartItemIds, request.PaymentMethod));
        return CreatedAtAction(nameof(CheckoutSelected), new { id = result.OrderId }, result);
    }


    /// Trích xuất UserId từ JWT Token
    private Guid GetUserIdFromToken()
    {
        // JwtBearer may map "sub" to NameIdentifier; accept both.
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Unable to extract user ID from token.");
        return userId;
    }
}

public class AddToCartCommandRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CheckoutRequest
{
    public PaymentMethod PaymentMethod { get; set; }
}

public class CheckoutSelectedRequest
{
    public List<Guid> CartItemIds { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
}
