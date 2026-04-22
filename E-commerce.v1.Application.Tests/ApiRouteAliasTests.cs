using System.Security.Claims;
using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCouponById;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCoupons;
using E_commerce.v1.Application.Features.Order.Commands.CancelOrder;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrderById;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRuleById;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRules;
using E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariantById;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariants;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.Application.Tests;

/// <summary>Smoke-tests controller actions dispatch expected commands/queries.</summary>
public class ApiRouteAliasTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrderId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ReviewId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task cart_checkout_without_cartItemIds_should_dispatch_CheckoutCommand()
    {
        var (ctrl, mediator) = CreateCart();

        var request = new CheckoutRequest { PaymentMethod = (int)PaymentMethod.Cod, Shipping = null, CartItemIds = null };
        await ctrl.Checkout(request);

        var cmd = Assert.IsType<CheckoutCommand>(mediator.LastSent);
        Assert.Equal(UserId, cmd.UserId);
        Assert.Equal(PaymentMethod.Cod, cmd.PaymentMethod);
    }

    [Fact]
    public async Task cart_checkout_with_cartItemIds_should_dispatch_CheckoutSelectedCommand()
    {
        var (ctrl, mediator) = CreateCart();

        var itemId = Guid.NewGuid();
        var request = new CheckoutRequest
        {
            PaymentMethod = (int)PaymentMethod.Cod,
            Shipping = null,
            CartItemIds = new List<Guid> { itemId }
        };
        await ctrl.Checkout(request);

        var cmd = Assert.IsType<CheckoutSelectedCommand>(mediator.LastSent);
        Assert.Equal(UserId, cmd.UserId);
        Assert.Equal(new[] { itemId }, cmd.CartItemIds);
    }

    [Fact]
    public async Task cart_checkout_should_return_CreatedAtAction_pointing_to_Order_GetOrderById()
    {
        var (ctrl, _) = CreateCart();
        var request = new CheckoutRequest { PaymentMethod = (int)PaymentMethod.Cod };

        var actionResult = await ctrl.Checkout(request);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(OrderController.GetOrderById), created.ActionName);
        Assert.Equal("Order", created.ControllerName);
        Assert.Equal(OrderId, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task orders_cancel_should_dispatch_CancelOrderCommand_with_owner()
    {
        var (ctrl, mediator) = CreateOrder();

        await ctrl.CancelOrder(OrderId);

        var cmd = Assert.IsType<CancelOrderCommand>(mediator.LastSent);
        Assert.Equal(UserId, cmd.UserId);
        Assert.Equal(OrderId, cmd.OrderId);
    }

    [Fact]
    public async Task admin_orders_list_and_detail_should_dispatch_admin_queries()
    {
        var (ctrl, mediator) = CreateAdminOrders();

        await ctrl.List(status: (int)OrderStatus.Pending, pageNumber: 2, pageSize: 5);
        var listCmd = Assert.IsType<GetAdminOrdersQuery>(mediator.LastSent);
        Assert.Equal(OrderStatus.Pending, listCmd.Status);
        Assert.Equal(2, listCmd.PageNumber);
        Assert.Equal(5, listCmd.PageSize);

        await ctrl.GetById(OrderId);
        var byIdCmd = Assert.IsType<GetAdminOrderByIdQuery>(mediator.LastSent);
        Assert.Equal(OrderId, byIdCmd.OrderId);
    }

    [Fact]
    public async Task admin_coupons_list_and_detail_should_dispatch_coupon_queries()
    {
        var (ctrl, mediator) = CreateAdminCoupons();

        await ctrl.List(code: "SALE", isActive: true, pageNumber: 3, pageSize: 50);
        var listCmd = Assert.IsType<GetCouponsQuery>(mediator.LastSent);
        Assert.Equal("SALE", listCmd.Code);
        Assert.True(listCmd.IsActive);

        var couponId = Guid.NewGuid();
        await ctrl.GetById(couponId);
        var byIdCmd = Assert.IsType<GetCouponByIdQuery>(mediator.LastSent);
        Assert.Equal(couponId, byIdCmd.Id);
    }

    [Fact]
    public async Task admin_promotions_list_and_detail_should_dispatch_promotion_queries()
    {
        var (ctrl, mediator) = CreateAdminPromotions();

        await ctrl.List(isActive: true, pageNumber: 1, pageSize: 10);
        Assert.IsType<GetPromotionRulesQuery>(mediator.LastSent);

        var ruleId = Guid.NewGuid();
        await ctrl.GetById(ruleId);
        var byIdCmd = Assert.IsType<GetPromotionRuleByIdQuery>(mediator.LastSent);
        Assert.Equal(ruleId, byIdCmd.Id);
    }

    [Fact]
    public async Task admin_variants_list_and_detail_should_dispatch_variant_queries()
    {
        var (ctrl, mediator) = CreateAdminVariants();

        var productId = Guid.NewGuid();
        await ctrl.List(productId: productId, isActive: null, pageNumber: 1, pageSize: 10);
        var listCmd = Assert.IsType<GetVariantsQuery>(mediator.LastSent);
        Assert.Equal(productId, listCmd.ProductId);

        var variantId = Guid.NewGuid();
        await ctrl.GetById(variantId);
        var byIdCmd = Assert.IsType<GetVariantByIdQuery>(mediator.LastSent);
        Assert.Equal(variantId, byIdCmd.Id);
    }

    private static (CartController ctrl, CapturingMediator mediator) CreateCart()
    {
        var mediator = new CapturingMediator
        {
            Response = new CheckoutResponse { OrderId = OrderId, OrderNumber = "ORD-TEST" }
        };
        var ctrl = new CartController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (OrderController ctrl, CapturingMediator mediator) CreateOrder()
    {
        var mediator = new CapturingMediator
        {
            Response = new CreateShipmentResponse { AhamoveOrderId = "AH", Status = "OK" }
        };
        var ctrl = new OrderController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (AdminProductsController ctrl, CapturingMediator mediator) CreateAdminProducts()
    {
        var mediator = new CapturingMediator { Response = Guid.NewGuid() };
        var ctrl = new AdminProductsController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (AdminOrdersController ctrl, CapturingMediator mediator) CreateAdminOrders()
    {
        var mediator = new CapturingMediator
        {
            Response = new PagedResult<OrderDto>()
        };
        var ctrl = new AdminOrdersController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (AdminCouponsController ctrl, CapturingMediator mediator) CreateAdminCoupons()
    {
        var mediator = new CapturingMediator
        {
            Response = new PagedResult<CouponDto>()
        };
        var ctrl = new AdminCouponsController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (AdminPromotionsController ctrl, CapturingMediator mediator) CreateAdminPromotions()
    {
        var mediator = new CapturingMediator
        {
            Response = new PagedResult<PromotionRuleDto>()
        };
        var ctrl = new AdminPromotionsController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (AdminVariantsController ctrl, CapturingMediator mediator) CreateAdminVariants()
    {
        var mediator = new CapturingMediator
        {
            Response = new PagedResult<ProductVariantDto>()
        };
        var ctrl = new AdminVariantsController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static void AttachUser(ControllerBase ctrl, bool authenticated = true)
    {
        var identity = authenticated
            ? new ClaimsIdentity(new[] { new Claim("sub", UserId.ToString()) }, "TestAuth")
            : new ClaimsIdentity();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        ctrl.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private sealed class CapturingMediator : IMediator
    {
        public object? LastSent { get; private set; }
        public object? Response { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastSent = request;
            var resp = Response is TResponse typed ? typed : default!;
            return Task.FromResult(resp);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            LastSent = request;
            return Task.FromResult<object?>(Response);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            LastSent = request!;
            return Task.CompletedTask;
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
            => Task.CompletedTask;
    }
}
