using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Application.Features.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly IAppDbContext _context;

    public AddToCartCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Không track Product: tránh xung đột tracker khi SaveChanges cùng Cart/CartItem (lần thêm 2+ hay gặp 500).
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException("Sản phẩm không tồn tại.");

        if (!product.IsActive)
            throw new BadRequestException("Sản phẩm đã ngừng bán, không thể thêm vào giỏ.");

        if (product.Stock < request.Quantity)
            throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            throw new BadRequestException("Tài khoản không tồn tại trong hệ thống. Vui lòng đăng nhập lại.");

        const int maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0)
                _context.ClearChangeTracker();

            // Không Include CartItems: tránh lỗi tracker khi thêm dòng thứ 2+ (Include + Add vào collection hay gây 500).
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart == null)
            {
                cart = new E_commerce.v1.Domain.Entities.Cart { UserId = request.UserId };
                _context.Carts.Add(cart);
            }

            var lineItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync(cancellationToken);

            var existingItem = lineItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (existingItem != null)
            {
                if (product.Stock < existingItem.Quantity + request.Quantity)
                    throw new BadRequestException("Số lượng cộng dồn vượt quá tồn kho.");

                existingItem.Quantity += request.Quantity;
            }
            else
            {
                _context.CartItems.Add(new E_commerce.v1.Domain.Entities.CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            catch (DbUpdateException ex)
            {
                if (IsSqlServerUniqueConstraintViolation(ex) && attempt < maxAttempts - 1)
                    continue;

                if (IsSqlServerForeignKeyViolation(ex))
                    throw new BadRequestException(
                        "Không thể thêm vào giỏ: dữ liệu không khớp (sản phẩm hoặc tài khoản). Hãy thử đăng nhập lại hoặc chọn sản phẩm từ danh sách hiện tại.");

                throw;
            }
        }

        throw new InvalidOperationException("Không thể thêm vào giỏ sau khi xử lý trùng khóa.");
    }

    /// <summary>SQL Server 2601/2627 — trùng giỏ (UserId) hoặc trùng dòng (CartId, ProductId).</summary>
    private static bool IsSqlServerUniqueConstraintViolation(DbUpdateException ex)
    {
        foreach (var n in GetSqlExceptionNumbers(ex))
        {
            if (n is 2601 or 2627)
                return true;
        }

        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
        {
            var msg = inner.Message;
            if (msg.Contains("IX_CartItems", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_Carts_UserId", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsSqlServerForeignKeyViolation(DbUpdateException ex)
    {
        foreach (var n in GetSqlExceptionNumbers(ex))
        {
            if (n == 547)
                return true;
        }

        return false;
    }

    private static IEnumerable<int> GetSqlExceptionNumbers(Exception ex)
    {
        for (var inner = ex; inner != null; inner = inner.InnerException!)
        {
            if (inner.GetType().Name != "SqlException")
                continue;
            var number = inner.GetType().GetProperty("Number")?.GetValue(inner);
            if (number is int i)
                yield return i;
            else if (number is long ln)
                yield return (int)ln;
        }
    }
}
