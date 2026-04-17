using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Promotion;
using E_commerce.v1.Application.Features.Promotions.Commands.CreatePromotionRule;
using E_commerce.v1.Application.Features.Promotions.Commands.DeletePromotionRule;
using E_commerce.v1.Application.Features.Promotions.Commands.UpdatePromotionRule;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRuleById;
using E_commerce.v1.Application.Features.Promotions.Queries.GetPromotionRules;
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

    [HttpGet]
    public async Task<ActionResult<PagedResult<PromotionRuleDto>>> List(
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetPromotionRulesQuery(isActive, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromotionRuleDetailDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPromotionRuleByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePromotionRuleCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] PromotionRuleUpsertDto rule)
    {
        await _mediator.Send(new UpdatePromotionRuleCommand(id, rule));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePromotionRuleCommand(id));
        return NoContent();
    }
}
