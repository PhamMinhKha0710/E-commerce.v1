using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _context.Users.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return _context.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
