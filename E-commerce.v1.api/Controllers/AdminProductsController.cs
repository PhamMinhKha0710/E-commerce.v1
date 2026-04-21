using E_commerce.v1.Application.Features.Products.Commands.CreateProduct;
using E_commerce.v1.Application.Features.Products.Commands.DeleteProduct;
using E_commerce.v1.Application.Features.Products.Commands.UpdateProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.v1.api.Controllers;

/// <summary>
/// CRUD sản phẩm dành riêng cho Admin. Các GET (list/detail/reviews) vẫn public ở
/// <c>api/v1/products</c> để storefront truy cập — tách controller để áp dụng
/// <c>Authorize(Roles = "Admin")</c> ở cấp class thay vì annotate từng action.
/// </summary>
[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateProductCommand command)
    {
        var productId = await _mediator.Send(command);
        return CreatedAtAction(
            actionName: nameof(ProductsController.GetProductById),
            controllerName: "Products",
            routeValues: new { id = productId },
            value: productId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        await _mediator.Send(command with { Id = id });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}
