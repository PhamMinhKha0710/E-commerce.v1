using E_commerce.v1.Application.DTOs.Cart;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

/// <summary>Merge giỏ guest (localStorage) vào giỏ DB: quantity = DB + local cho từng sản phẩm.</summary>
public record SyncCartCommand(Guid UserId, IReadOnlyList<SyncCartLineDto> Items) : IRequest<CartDto>;
