using E_commerce.v1.Application.DTOs.Cart;
using MediatR;

namespace E_commerce.v1.Application.Features.Cart.Commands;

/// <summary>
/// Merge giỏ guest (lưu localStorage) vào giỏ DB sau khi login để không mất item:
/// với mỗi sản phẩm, quantity mới = quantity(DB) + quantity(guest).
/// </summary>
public record SyncCartCommand(Guid UserId, IReadOnlyList<SyncCartLineDto> Items) : IRequest<CartDto>;
