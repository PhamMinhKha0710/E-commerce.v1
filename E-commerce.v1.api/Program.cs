using E_commerce.v1.api.Middlewares;
using E_commerce.v1.Application;
using E_commerce.v1.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Domain.Entities;
using E_commerce.v1.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at apps root
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Data Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Trong môi trường development, tự động tạo DB nếu chưa có
    context.Database.EnsureCreated();
    
    if (!context.Roles.Any())
    {
        var adminRole = new Role { Name = "Admin", Description = "Administrator" };
        var userRole = new Role { Name = "User", Description = "Regular User" };
        context.Roles.AddRange(adminRole, userRole);
        context.SaveChanges();

        if (!context.Users.Any())
        {
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
    }
}

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
