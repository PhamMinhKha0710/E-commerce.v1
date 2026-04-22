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

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = User.GetRequiredUserId();
        var query = new GetCartQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Merge giỏ guest (lưu ở localStorage phía FE) vào giỏ DB ngay sau khi user login,
    /// để các item thêm lúc chưa đăng nhập không bị mất.
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<CartDto>> SyncCart([FromBody] SyncCartRequest request)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new SyncCartCommand(userId, request.Items));
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartActionResponse>> AddToCartItems([FromBody] AddToCartCommandRequest request)
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

    /// <summary>
    /// Cập nhật số lượng cart item. Khi <c>Quantity = 0</c>, endpoint sẽ xóa luôn
    /// dòng đó (tương đương DELETE) để FE chỉ cần 1 action cho stepper +/-.
    /// </summary>
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

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveCartItem(Guid id)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new RemoveCartItemCommand(userId, id));
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new ClearCartCommand(userId));
        return NoContent();
    }

    /// <summary>
    /// Checkout giỏ hàng. Có <c>cartItemIds</c> = chỉ checkout các item được chọn
    /// (case user tick một vài sản phẩm); bỏ trống = checkout toàn bộ giỏ.
    /// Một endpoint xử lý cả hai case để FE không cần phân nhánh route.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request)
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

    /// <summary>Có giá trị = chỉ checkout các cart item được chọn; null/empty = checkout toàn bộ giỏ.</summary>
    public List<Guid>? CartItemIds { get; set; }
}

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
}

public sealed record CartActionResponse(string Message);
