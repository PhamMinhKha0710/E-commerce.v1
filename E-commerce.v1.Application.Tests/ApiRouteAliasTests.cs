using System.Security.Claims;
using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Coupon;
using E_commerce.v1.Application.DTOs.Order;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Features.Cart.Commands;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCouponById;
using E_commerce.v1.Application.Features.Coupons.Queries.GetCoupons;
using E_commerce.v1.Application.Features.Order.Commands.CancelOrder;
using E_commerce.v1.Application.Features.Order.Commands.Checkout;
using E_commerce.v1.Application.Features.Order.Commands.CheckoutSelected;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrderById;
using E_commerce.v1.Application.Features.Order.Queries.Admin.GetAdminOrders;
using E_commerce.v1.Application.Features.Payment.Commands.CreatePayosPaymentLink;
using E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;
using E_commerce.v1.Application.Features.Products.Commands.CreateProduct;
using E_commerce.v1.Application.Features.Products.Commands.DeleteProduct;
using E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRuleById;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRules;
using E_commerce.v1.Application.Features.Reviews.Commands.PostReview;
using E_commerce.v1.Application.Features.Shipping.Commands.CreateShipment;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariantById;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariants;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.Application.Tests;

/// <summary>
/// Verifies that legacy (Obsolete) routes dispatch the same command / have the same
/// observable effect as the new canonical routes after the API consolidation.
/// </summary>
public class ApiRouteAliasTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OrderId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ReviewId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task cart_add_and_cart_items_should_dispatch_same_AddToCartCommand()
    {
        var (newCtrl, newMediator) = CreateCart();
        var (legacyCtrl, legacyMediator) = CreateCart();

        var body = new AddToCartCommandRequest { ProductId = ProductId, Quantity = 2 };

        await newCtrl.AddToCartItems(body);
#pragma warning disable CS0618
        await legacyCtrl.AddToCart(body);
#pragma warning restore CS0618

        var expected = new AddToCartCommand { UserId = UserId, ProductId = ProductId, Quantity = 2 };
        AssertCart(newMediator.LastSent, expected);
        AssertCart(legacyMediator.LastSent, expected);
    }

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
    public async Task cart_checkout_selected_legacy_route_should_dispatch_CheckoutSelectedCommand()
    {
        var (ctrl, mediator) = CreateCart();
        var itemId = Guid.NewGuid();
        var request = new CheckoutSelectedRequest
        {
            CartItemIds = new List<Guid> { itemId },
            PaymentMethod = (int)PaymentMethod.Cod,
            Shipping = null
        };

#pragma warning disable CS0618
        await ctrl.CheckoutSelected(request);
#pragma warning restore CS0618

        var cmd = Assert.IsType<CheckoutSelectedCommand>(mediator.LastSent);
        Assert.Equal(new[] { itemId }, cmd.CartItemIds);
    }

    [Fact]
    public async Task orders_shipment_and_create_shipment_should_dispatch_same_CreateShipmentCommand()
    {
        var (newCtrl, newMediator) = CreateOrder();
        var (legacyCtrl, legacyMediator) = CreateOrder();
        var body = new CreateShipmentRequest { ServiceId = "SGN-BIKE" };

        await newCtrl.CreateShipment(OrderId, body);
#pragma warning disable CS0618
        await legacyCtrl.CreateShipmentLegacy(OrderId, body);
#pragma warning restore CS0618

        AssertShipment(newMediator.LastSent, OrderId, body);
        AssertShipment(legacyMediator.LastSent, OrderId, body);
    }

    [Fact]
    public async Task payment_payos_root_and_create_should_dispatch_same_CreatePayosPaymentLinkCommand()
    {
        var (newCtrl, newMediator) = CreatePayos();
        var (legacyCtrl, legacyMediator) = CreatePayos();
        var body = new CreatePayosPaymentLinkRequest { OrderId = OrderId, TotalAmount = 100_000m, Description = "demo" };

        await newCtrl.Create(body);
#pragma warning disable CS0618
        await legacyCtrl.CreateLegacy(body);
#pragma warning restore CS0618

        AssertPayos(newMediator.LastSent, OrderId);
        AssertPayos(legacyMediator.LastSent, OrderId);
    }

    [Fact]
    public async Task payos_webhook_new_and_legacy_routes_should_dispatch_ProcessPayosWebhookCommand()
    {
        var (newCtrl, newMediator) = CreateWebhook(authenticated: false);
        var (legacyCtrl, legacyMediator) = CreateWebhook(authenticated: false);

        var payload = System.Text.Json.JsonSerializer.SerializeToElement(new { data = new { orderCode = 1 }, signature = "sig" });

        await newCtrl.PostNew(payload);
#pragma warning disable CS0618
        await legacyCtrl.Post(payload);
#pragma warning restore CS0618

        Assert.IsType<ProcessPayosWebhookCommand>(newMediator.LastSent);
        Assert.IsType<ProcessPayosWebhookCommand>(legacyMediator.LastSent);
    }

    [Fact]
    public async Task reviews_nested_and_flat_routes_should_dispatch_same_PostReviewCommand()
    {
        var (productsCtrl, productsMediator) = CreateProducts();
        var (reviewsCtrl, reviewsMediator) = CreateReviews();

        await productsCtrl.PostProductReview(ProductId, new PostProductReviewRequest { Rating = 5, Comment = "great" });
#pragma warning disable CS0618
        await reviewsCtrl.PostReview(new PostReviewCommand { ProductId = ProductId, Rating = 5, Comment = "great" });
#pragma warning restore CS0618

        var nested = Assert.IsType<PostReviewCommand>(productsMediator.LastSent);
        var flat = Assert.IsType<PostReviewCommand>(reviewsMediator.LastSent);
        Assert.Equal(ProductId, nested.ProductId);
        Assert.Equal(ProductId, flat.ProductId);
        Assert.Equal(UserId, nested.UserId);
        Assert.Equal(UserId, flat.UserId);
        Assert.Equal(5, nested.Rating);
        Assert.Equal(5, flat.Rating);
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

    [Fact]
    public async Task admin_products_and_products_admin_actions_should_dispatch_equivalent_commands()
    {
        var (adminCtrl, adminMediator) = CreateAdminProducts();
        var (productsCtrl, productsMediator) = CreateProducts();

        var createCmd = new CreateProductCommand("name", null, 100m, 1, Guid.NewGuid());

        await adminCtrl.Create(createCmd);
#pragma warning disable CS0618
        await productsCtrl.CreateProduct(createCmd);
#pragma warning restore CS0618

        Assert.IsType<CreateProductCommand>(adminMediator.LastSent);
        Assert.IsType<CreateProductCommand>(productsMediator.LastSent);

        var updateId = Guid.NewGuid();
        var updateCmd = new UpdateProductCommand(updateId, "n", null, 1m, 0, Guid.NewGuid());
        await adminCtrl.Update(updateId, updateCmd);
#pragma warning disable CS0618
        await productsCtrl.UpdateProduct(updateId, updateCmd);
#pragma warning restore CS0618
        Assert.IsType<UpdateProductCommand>(adminMediator.LastSent);
        Assert.IsType<UpdateProductCommand>(productsMediator.LastSent);

        var deleteId = Guid.NewGuid();
        await adminCtrl.Delete(deleteId);
#pragma warning disable CS0618
        await productsCtrl.DeleteProduct(deleteId);
#pragma warning restore CS0618
        Assert.Equal(deleteId, Assert.IsType<DeleteProductCommand>(adminMediator.LastSent).Id);
        Assert.Equal(deleteId, Assert.IsType<DeleteProductCommand>(productsMediator.LastSent).Id);
    }

    private static void AssertCart(object? sent, AddToCartCommand expected)
    {
        var cmd = Assert.IsType<AddToCartCommand>(sent);
        Assert.Equal(expected.UserId, cmd.UserId);
        Assert.Equal(expected.ProductId, cmd.ProductId);
        Assert.Equal(expected.Quantity, cmd.Quantity);
    }

    private static void AssertShipment(object? sent, Guid orderId, CreateShipmentRequest body)
    {
        var cmd = Assert.IsType<CreateShipmentCommand>(sent);
        Assert.Equal(UserId, cmd.UserId);
        Assert.Equal(orderId, cmd.OrderId);
        Assert.Same(body, cmd.Body);
    }

    private static void AssertPayos(object? sent, Guid orderId)
    {
        var cmd = Assert.IsType<CreatePayosPaymentLinkCommand>(sent);
        Assert.Equal(UserId, cmd.UserId);
        Assert.Equal(orderId, cmd.OrderId);
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

    private static (PayosPaymentController ctrl, CapturingMediator mediator) CreatePayos()
    {
        var mediator = new CapturingMediator
        {
            Response = new CreatePayosPaymentLinkResponse { PaymentLinkId = "pl", PaymentUrl = "http://example" }
        };
        var ctrl = new PayosPaymentController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (PayosWebhookController ctrl, CapturingMediator mediator) CreateWebhook(bool authenticated)
    {
        var mediator = new CapturingMediator { Response = Unit.Value };
        var ctrl = new PayosWebhookController(mediator);
        AttachUser(ctrl, authenticated);
        return (ctrl, mediator);
    }

    private static (ProductsController ctrl, CapturingMediator mediator) CreateProducts()
    {
        var mediator = new CapturingMediator { Response = ReviewId };
        var ctrl = new ProductsController(mediator);
        AttachUser(ctrl);
        return (ctrl, mediator);
    }

    private static (ReviewsController ctrl, CapturingMediator mediator) CreateReviews()
    {
        var mediator = new CapturingMediator { Response = ReviewId };
        var ctrl = new ReviewsController(mediator);
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
