using E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;
using E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;
using E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

[ApiController]
[Route("api/v1/admin/variants")]
[Authorize(Roles = "Admin")]
public class AdminVariantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminVariantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateVariantCommand command)
    {
        var variantId = await _mediator.Send(command);
        return Ok(variantId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateVariantCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID trên URL không khớp với dữ liệu." });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVariantCommand(id));
        return NoContent();
    }
}

