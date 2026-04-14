using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Domain.Enums;
using E_commerce.v1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.v1.Infrastructure.Seeding;

public static class AppDbContextSeeder
{
    public static void MigrateAndSeed(AppDbContext context)
    {
        context.Database.Migrate();

        SeedRolesAndUsers(context);
        SeedCategoriesAndProducts(context);
    }

    private static void SeedRolesAndUsers(AppDbContext context)
    {
        if (context.Roles.Any())
            return;

        var adminRole = new Role { Name = "Admin", Description = "Administrator" };
        var userRole = new Role { Name = "User", Description = "Regular User" };
        context.Roles.AddRange(adminRole, userRole);
        context.SaveChanges();

        if (context.Users.Any())
            return;

        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "System",
            Email = "admin@mebisoft.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true
        };
        adminUser.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
        context.Users.Add(adminUser);
        context.SaveChanges();
    }

    private static void SeedCategoriesAndProducts(AppDbContext context)
    {
        if (context.Categories.Any())
            return;

        var category1 = new Category
        {
            Name = "Laptop",
            Description = "Máy tính xách tay",
            Slug = "laptop",
            Image = "https://cdn.example.com/categories/laptop.jpg",
            IsActive = true
        };
        var category2 = new Category
        {
            Name = "Điện thoại",
            Description = "Smartphones",
            Slug = "dien-thoai",
            Image = "https://cdn.example.com/categories/dien-thoai.jpg",
            IsActive = true
        };
        context.Categories.AddRange(category1, category2);
        context.SaveChanges();

        var childLaptop = new Category
        {
            Name = "Laptop gaming",
            Description = "Laptop hiệu năng cao",
            Slug = "laptop-gaming",
            IsActive = true,
            ParentCategoryId = category1.Id
        };
        context.Categories.Add(childLaptop);
        context.SaveChanges();

        if (context.Products.Any())
            return;

        context.Products.AddRange(
            new Product
            {
                Name = "MacBook Pro M3",
                NameEn = "MacBook Pro M3",
                Sku = "MB-M3-001",
                Slug = "macbook-pro-m3",
                Description = "MacBook Pro chip M3",
                Price = 35000000,
                Discount = 0,
                Stock = 10,
                Unit = "chiếc",
                UnitEn = "unit",
                IsActive = true,
                ProductType = ProductType.Single,
                Kind = ProductKind.Sale,
                BuyType = BuyType.All,
                Priority = 2,
                DocumentIds = new List<string> { "img-mb-001" },
                CategoryId = category1.Id
            },
            new Product
            {
                Name = "Dell XPS 15",
                Sku = "DL-XPS-015",
                Slug = "dell-xps-15",
                Price = 30000000,
                Discount = 500000,
                Stock = 5,
                IsActive = true,
                ProductType = ProductType.Single,
                Kind = ProductKind.Promotion,
                DocumentIds = new List<string>(),
                CategoryId = category1.Id
            },
            new Product
            {
                Name = "iPhone 15 Pro",
                Sku = "IP-15P-001",
                Slug = "iphone-15-pro",
                Price = 25000000,
                Stock = 20,
                IsActive = true,
                ProductType = ProductType.Single,
                Kind = ProductKind.Sale,
                CategoryId = category2.Id
            },
            new Product
            {
                Name = "Samsung Galaxy S24",
                Sku = "SG-S24-001",
                Slug = "samsung-galaxy-s24",
                Price = 22000000,
                Stock = 15,
                IsActive = true,
                ProductType = ProductType.Single,
                Kind = ProductKind.Sale,
                CategoryId = category2.Id
            });
        context.SaveChanges();
    }
}
