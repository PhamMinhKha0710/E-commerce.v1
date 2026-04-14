using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Exceptions;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null) throw new NotFoundException("Sản phẩm không tồn tại.");
        if (!product.IsActive) throw new BadRequestException("Sản phẩm đã ngừng bán, không thể thêm vào giỏ.");
        if (product.Stock < quantity) throw new BadRequestException($"Không đủ hàng. Kho chỉ còn {product.Stock} sản phẩm.");

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists) throw new BadRequestException("Tài khoản không tồn tại trong hệ thống. Vui lòng đăng nhập lại.");

        const int maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0) _context.ChangeTracker.Clear();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var lineItems = await _context.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync(cancellationToken);
            var existingItem = lineItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                if (product.Stock < existingItem.Quantity + quantity) throw new BadRequestException("Số lượng cộng dồn vượt quá tồn kho.");
                existingItem.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = productId, Quantity = quantity });
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateException ex)
            {
                if (IsSqlServerUniqueConstraintViolation(ex) && attempt < maxAttempts - 1) continue;
                if (IsSqlServerForeignKeyViolation(ex))
                    throw new BadRequestException("Không thể thêm vào giỏ: dữ liệu không khớp (sản phẩm hoặc tài khoản). Hãy thử đăng nhập lại hoặc chọn sản phẩm từ danh sách hiện tại.");
                throw;
            }
        }

        throw new InvalidOperationException("Không thể thêm vào giỏ sau khi xử lý trùng khóa.");
    }

    private static bool IsSqlServerUniqueConstraintViolation(DbUpdateException ex)
    {
        foreach (var n in GetSqlExceptionNumbers(ex))
        {
            if (n is 2601 or 2627) return true;
        }
        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
        {
            var msg = inner.Message;
            if (msg.Contains("IX_CartItems", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_Carts_UserId", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static bool IsSqlServerForeignKeyViolation(DbUpdateException ex)
    {
        foreach (var n in GetSqlExceptionNumbers(ex))
        {
            if (n == 547) return true;
        }
        return false;
    }

    private static IEnumerable<int> GetSqlExceptionNumbers(Exception ex)
    {
        for (var inner = ex; inner != null; inner = inner.InnerException!)
        {
            if (inner.GetType().Name != "SqlException") continue;
            var number = inner.GetType().GetProperty("Number")?.GetValue(inner);
            if (number is int i) yield return i;
            else if (number is long ln) yield return (int)ln;
        }
    }
}
