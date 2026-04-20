using E_commerce.v1.Application.DTOs.Common;
using E_commerce.v1.Application.DTOs.Product;
using E_commerce.v1.Application.Features.Variants.Commands.CreateVariant;
using E_commerce.v1.Application.Features.Variants.Commands.DeleteVariant;
using E_commerce.v1.Application.Features.Variants.Commands.UpdateVariant;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariantById;
using E_commerce.v1.Application.Features.Variants.Queries.GetVariants;
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

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductVariantDto>>> List(
        [FromQuery] Guid? productId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetVariantsQuery(productId, isActive, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductVariantDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetVariantByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateVariantCommand command)
    {
        var variantId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = variantId }, variantId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateVariantRequest request)
    {
        await _mediator.Send(new UpdateVariantCommand(
            id, request.Sku, request.Price, request.Inventory, request.IsActive, request.Options));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVariantCommand(id));
        return NoContent();
    }
}

public class UpdateVariantRequest
{
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Inventory { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ProductVariantOptionDto>? Options { get; set; }
}
