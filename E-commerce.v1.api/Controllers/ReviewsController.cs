using E_commerce.v1.Application.Features.Reviews.Commands.PostReview;
using E_commerce.v1.api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[Route("api/v1/reviews")]
[ApiController]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> PostReview([FromBody] PostReviewCommand command)
    {
        command.UserId = User.GetRequiredUserId();
        var reviewId = await _mediator.Send(command);
        return Ok(reviewId);
    }
}
