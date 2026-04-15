using E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;
using E_commerce.v1.Application.Features.Promotions.Commands.DeletePromotionRule;
using E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[ApiController]
[Route("api/v1/admin/promotions")]
[Authorize(Roles = "Admin")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPromotionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePromotionRuleCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePromotionRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID trên URL không khớp với dữ liệu." });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePromotionRuleCommand(id));
        return NoContent();
    }
}

