using E_commerce.v1.Application.DTOs.Cart;
using E_commerce.v1.Application.Features.Cart.Commands;
using E_commerce.v1.Application.Features.Cart.Queries;
using E_commerce.v1.Application.Features.Coupons.Commands.ApplyCouponToCart;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.api.Extensions;
using E_commerce.v1.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>Lấy giỏ hàng của user hiện tại.</summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = User.GetRequiredUserId();
        var query = new GetCartQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Đồng bộ giỏ guest (localStorage) vào DB sau khi đăng nhập.
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<CartDto>> SyncCart([FromBody] SyncCartRequest request)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new SyncCartCommand(userId, request.Items));
        return Ok(result);
    }

    /// <summary>Thêm sản phẩm vào giỏ hàng (RESTful).</summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartActionResponse>> AddToCartItems([FromBody] AddToCartCommandRequest request)
        => await AddToCartInternal(request);

    /// <summary>Thêm sản phẩm vào giỏ hàng (deprecated, dùng POST api/v1/cart/items).</summary>
    [HttpPost("add")]
    [Obsolete("Use POST api/v1/cart/items instead.")]
    public async Task<ActionResult<CartActionResponse>> AddToCart([FromBody] AddToCartCommandRequest request)
        => await AddToCartInternal(request);

    private async Task<ActionResult<CartActionResponse>> AddToCartInternal(AddToCartCommandRequest request)
    {
        var userId = User.GetRequiredUserId();
        var command = new AddToCartCommand
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        await _mediator.Send(command);
        return Ok(new CartActionResponse("Sản phẩm đã được thêm vào giỏ hàng thành công."));
    }

    /// <summary>Cập nhật số lượng sản phẩm trong giỏ hàng. Nếu Quantity = 0, dòng sẽ bị xóa.</summary>
    [HttpPut("items/{id:guid}")]
    public async Task<ActionResult<CartActionResponse>> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.GetRequiredUserId();
        var command = new UpdateCartItemCommand
        {
            UserId = userId,
            CartItemId = id,
            Quantity = request.Quantity
        };

        await _mediator.Send(command);
        return Ok(new CartActionResponse("Giỏ hàng đã được cập nhật thành công."));
    }

    /// <summary>Xóa một dòng trong giỏ (tương đương PUT quantity = 0).</summary>
    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveCartItem(Guid id)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new RemoveCartItemCommand(userId, id));
        return NoContent();
    }

    /// <summary>Xóa toàn bộ giỏ.</summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new ClearCartCommand(userId));
        return NoContent();
    }

    /// <summary>
    /// Checkout giỏ hàng. Khi có <c>cartItemIds</c> thì checkout các item được chọn;
    /// khi bỏ trống thì checkout toàn bộ giỏ hàng.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request)
        => await CheckoutInternal(request.CartItemIds, request.PaymentMethod, request.Shipping);

    /// <summary>Checkout các item được chọn (deprecated, dùng POST api/v1/cart/checkout với cartItemIds).</summary>
    [HttpPost("checkout-selected")]
    [Obsolete("Use POST api/v1/cart/checkout with cartItemIds instead.")]
    public async Task<ActionResult<CheckoutResponse>> CheckoutSelected([FromBody] CheckoutSelectedRequest request)
        => await CheckoutInternal(request.CartItemIds, request.PaymentMethod, request.Shipping);

    private async Task<ActionResult<CheckoutResponse>> CheckoutInternal(
        List<Guid>? cartItemIds,
        PaymentMethod paymentMethod,
        CheckoutShippingInfo? shipping)
    {
        var userId = User.GetRequiredUserId();

        var result = cartItemIds is { Count: > 0 }
            ? await _mediator.Send(new CheckoutSelectedCommand(userId, cartItemIds, paymentMethod, shipping))
            : await _mediator.Send(new CheckoutCommand(userId, paymentMethod, shipping));

        return CreatedAtAction(
            actionName: nameof(OrderController.GetOrderById),
            controllerName: "Order",
            routeValues: new { id = result.OrderId },
            value: result);
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<ApplyCouponToCartResponse>> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new ApplyCouponToCartCommand(userId, request.CouponCode));
        return Ok(result);
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
    public CheckoutShippingInfo? Shipping { get; set; }

    /// <summary>Khi có giá trị, chỉ checkout các cart item được chọn; bỏ trống = checkout toàn bộ giỏ.</summary>
    public List<Guid>? CartItemIds { get; set; }
}

public class CheckoutSelectedRequest
{
    public List<Guid> CartItemIds { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public CheckoutShippingInfo? Shipping { get; set; }
}

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
}

public sealed record CartActionResponse(string Message);
