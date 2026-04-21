using E_commerce.v1.Application.DTOs.Category;
using E_commerce.v1.Application.Features.Categories.Commands.CreateCategory;
using E_commerce.v1.Application.Features.Categories.Commands.DeleteCategory;
using E_commerce.v1.Application.Features.Categories.Commands.UpdateCategory;
using E_commerce.v1.Application.Features.Categories.Queries.GetCategories;
using E_commerce.v1.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

/// <summary>
/// Admin-only CRUD cho danh mục sản phẩm. GET công khai ở <c>api/v1/categories</c>.
/// </summary>
[ApiController]
[Route("api/v1/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryListDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
