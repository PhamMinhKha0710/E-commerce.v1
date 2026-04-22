using E_commerce.v1.Application.Features.Reviews.Commands.PostReview;
using E_commerce.v1.Application.Features.Reviews.Queries.GetProductReviews;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Services;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace E_commerce.v1.Application.Tests;

public class ReviewHandlersTests
{
    [Fact]
    public async Task should_reject_post_review_when_user_has_no_completed_order_for_product()
    {
        await using var context = CreateContext();
        var product = new Product { Name = "Milk", Sku = "MILK-1", Slug = "milk-1", Price = 10, CategoryId = Guid.NewGuid() };
        context.Categories.Add(new Category { Id = product.CategoryId, Name = "Food", Slug = "food" });
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var reviewRepository = new ReviewRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var reviewService = new ReviewService(reviewRepository, unitOfWork, NullLogger<ReviewService>.Instance);
        var handler = new PostReviewCommandHandler(reviewService);
        var command = new PostReviewCommand
        {
            ProductId = product.Id,
            UserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great product"
        };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task should_upsert_review_when_same_user_posts_again()
    {
        await using var context = CreateContext();
        var seed = await SeedCompletedOrderWithProduct(context);
        var reviewRepository = new ReviewRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var reviewService = new ReviewService(reviewRepository, unitOfWork, NullLogger<ReviewService>.Instance);
        var handler = new PostReviewCommandHandler(reviewService);

        var firstReviewId = await handler.Handle(
            new PostReviewCommand
            {
                ProductId = seed.ProductId,
                UserId = seed.UserId,
                Rating = 4,
                Comment = "Good"
            },
            CancellationToken.None);

        var secondReviewId = await handler.Handle(
            new PostReviewCommand
            {
                ProductId = seed.ProductId,
                UserId = seed.UserId,
                Rating = 5,
                Comment = "Excellent"
            },
            CancellationToken.None);

        Assert.Equal(firstReviewId, secondReviewId);

        var savedReview = await context.Reviews.SingleAsync(r => r.ProductId == seed.ProductId && r.UserId == seed.UserId);
        Assert.Equal(5, savedReview.Rating);
        Assert.Equal("Excellent", savedReview.Comment);
    }

    [Fact]
    public async Task should_return_paginated_reviews_with_average_rating()
    {
        await using var context = CreateContext();
        var seed = await SeedCompletedOrderWithProduct(context);

        context.Reviews.AddRange(
            new Review { ProductId = seed.ProductId, UserId = seed.UserId, Rating = 5, Comment = "Perfect" },
            new Review
            {
                ProductId = seed.ProductId,
                UserId = seed.SecondUserId,
                Rating = 3,
                Comment = "Okay"
            });
        await context.SaveChangesAsync();

        var reviewReadRepository = new ReviewReadRepository(context);
        var handler = new GetProductReviewsQueryHandler(reviewReadRepository);
        var result = await handler.Handle(
            new GetProductReviewsQuery(seed.ProductId, Page: 1, PageNumber: null, PageSize: 1),
            CancellationToken.None);

        Assert.Equal(2, result.TotalReviews);
        Assert.Equal(4.00m, result.AverageRating);
        Assert.Single(result.Reviews.Items);
        Assert.Equal(2, result.Reviews.TotalCount);
        Assert.Equal(1, result.Reviews.PageNumber);
        Assert.Equal(1, result.Reviews.PageSize);
    }

    private static async Task<(Guid ProductId, Guid UserId, Guid SecondUserId)> SeedCompletedOrderWithProduct(AppDbContext context)
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var secondOrderId = Guid.NewGuid();

        context.Categories.Add(new Category { Id = categoryId, Name = "Food", Slug = "food" });
        context.Users.AddRange(
            new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@example.com", PasswordHash = "hash" },
            new User { Id = secondUserId, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", PasswordHash = "hash" });
        context.Products.Add(new Product
        {
            Id = productId,
            Name = "Bread",
            Sku = "BRD-1",
            Slug = "bread-1",
            Price = 15,
            CategoryId = categoryId
        });
        context.Orders.AddRange(
            new Order { Id = orderId, UserId = userId, OrderNumber = "ORD-1", Status = OrderStatus.Completed, GrandTotal = 15 },
            new Order { Id = secondOrderId, UserId = secondUserId, OrderNumber = "ORD-2", Status = OrderStatus.Completed, GrandTotal = 15 });
        context.OrderItems.AddRange(
            new OrderItem { OrderId = orderId, ProductId = productId, Quantity = 1, UnitPrice = 15, LineTotal = 15, ProductNameSnapshot = "Bread" },
            new OrderItem { OrderId = secondOrderId, ProductId = productId, Quantity = 1, UnitPrice = 15, LineTotal = 15, ProductNameSnapshot = "Bread" });
        await context.SaveChangesAsync();

        return (productId, userId, secondUserId);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new FakeCurrentUserService());
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => Guid.NewGuid();
        public string? Email => "test@example.com";
        public string GetActor() => "test-user";
    }
}
