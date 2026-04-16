using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Services;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Tests;

public class CheckoutServiceShippingTests
{
    [Fact]
    public async Task should_calculate_shipping_fee_and_auto_create_shipment_when_shipping_provided()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        context.Users.Add(new User { Id = userId, Email = "u@test.com", LoyaltyRank = LoyaltyRank.Silver });
        context.Categories.Add(new Category { Id = categoryId, Name = "C" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "P",
            Price = 100_000m,
            Discount = 0,
            Stock = 10,
            IsActive = true,
            CategoryId = categoryId
        });

        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        cart.CartItems.Add(new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = productId, Quantity = 1 });
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var checkoutRepository = new CheckoutRepository(context);
        var promotionRuleReadRepository = new EmptyPromotionRuleReadRepository();
        var unitOfWork = new EfUnitOfWork(context);
        var orderRepository = new OrderRepository(context);

        var ahamove = new FakeAhamoveClient
        {
            EstimateResult = new List<AhamoveEstimateResultItem>
            {
                new()
                {
                    ServiceId = "SGN-BIKE",
                    Data = new AhamoveEstimateData { TotalFee = 15000m, Distance = 1, Duration = 1, DistanceFee = 1, RequestFee = 1 }
                }
            },
            CreateResult = new AhamoveCreateOrderResponse { OrderId = "AH123", Status = "assigning" }
        };

        var options = Options.Create(new AhamoveOptions
        {
            Pickup = new AhamovePickupOptions
            {
                Lat = 10.77,
                Lng = 106.66,
                Address = "Pickup address",
                Name = "Shop",
                Mobile = "0900000000"
            }
        });

        var service = new CheckoutService(
            checkoutRepository,
            promotionRuleReadRepository,
            unitOfWork,
            ahamove,
            options,
            orderRepository);

        var response = await service.CheckoutAsync(
            userId,
            PaymentMethod.Cod,
            new CheckoutShippingInfo
            {
                ServiceId = "SGN-BIKE",
                Dropoff = new AhamovePathPoint
                {
                    Lat = 10.78,
                    Lng = 106.67,
                    Address = "Dropoff address",
                    Name = "Receiver",
                    Mobile = "0911111111"
                },
                Note = "Call before arrive"
            },
            CancellationToken.None);

        Assert.Equal(15000m, response.ShippingFee);
        Assert.Equal("SGN-BIKE", response.ShippingServiceId);
        Assert.Equal("AH123", response.AhamoveOrderId);

        var order = await orderRepository.GetOrderByIdAsync(response.OrderId, CancellationToken.None);
        Assert.NotNull(order);
        Assert.Equal("Dropoff address", order!.ShippingAddressLine);
        Assert.Equal(15000m, order.ShippingFee);
        Assert.Equal("AH123", order.AhamoveOrderId);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options, new FakeCurrentUserService());
    }

    private sealed class EmptyPromotionRuleReadRepository : IPromotionRuleReadRepository
    {
        public Task<List<PromotionRule>> GetActiveRulesAsync(DateTime utcNow, CancellationToken cancellationToken)
            => Task.FromResult(new List<PromotionRule>());
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => Guid.NewGuid();
        public string? Email => "test@example.com";
        public string GetActor() => "test-user";
    }

    private sealed class FakeAhamoveClient : IAhamoveClient
    {
        public IReadOnlyList<AhamoveEstimateResultItem> EstimateResult { get; set; } = Array.Empty<AhamoveEstimateResultItem>();
        public AhamoveCreateOrderResponse CreateResult { get; set; } = new();

        public Task<IReadOnlyList<AhamoveEstimateResultItem>> EstimateAsync(AhamoveEstimateRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(EstimateResult);

        public Task<AhamoveCreateOrderResponse> CreateOrderAsync(AhamoveCreateOrderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResult);
    }
}

