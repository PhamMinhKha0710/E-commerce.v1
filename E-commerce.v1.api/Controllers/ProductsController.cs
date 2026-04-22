using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Features.Products.Queries.GetProductById;
using E_commerce.v1.Application.Features.Products.Queries.GetProducts;
using E_commerce.v1.Application.Features.Reviews.Commands.PostReview;
using E_commerce.v1.Application.Features.Reviews.Queries.GetProductReviews;
using E_commerce.v1.Application.DTOs.Review;
using E_commerce.v1.api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <param name="page">[Deprecated] Query cũ, client mới dùng <c>pageNumber</c>.</param>
    [HttpGet("{id:guid}/reviews")]
    public async Task<ActionResult<ProductReviewsSummaryDto>> GetProductReviews(
        Guid id,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? page = null)
    {
        var query = new GetProductReviewsQuery(id, page, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    public async Task<ActionResult<Guid>> PostProductReview(Guid id, [FromBody] PostProductReviewRequest request)
    {
        var command = new PostReviewCommand
        {
            ProductId = id,
            Rating = request.Rating,
            Comment = request.Comment,
            UserId = User.GetRequiredUserId()
        };
        var reviewId = await _mediator.Send(command);
        return Ok(reviewId);
    }
}

public sealed class PostProductReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
