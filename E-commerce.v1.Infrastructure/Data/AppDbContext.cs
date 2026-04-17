using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Common;
using E_commerce.v1.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace E_commerce.v1.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductVariantOption> ProductVariantOptions => Set<ProductVariantOption>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
    public DbSet<PromotionRuleProduct> PromotionRuleProducts => Set<PromotionRuleProduct>();
    public DbSet<PromotionRuleCategory> PromotionRuleCategories => Set<PromotionRuleCategory>();
    public DbSet<PromotionBuyXGetYAction> PromotionBuyXGetYActions => Set<PromotionBuyXGetYAction>();
    public DbSet<PromotionPercentageAction> PromotionPercentageActions => Set<PromotionPercentageAction>();

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public void ClearChangeTracker() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
            
        builder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
            
        builder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
        
        // Tự động tìm kiếm các file cấu hình ánh xạ DB (Configurations) trong project
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var actor = _currentUserService.GetActor();
        foreach (var entry in ChangeTracker.Entries<E_commerce.v1.Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = actor;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = actor;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
