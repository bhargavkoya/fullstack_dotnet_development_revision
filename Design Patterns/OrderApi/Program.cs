using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Domain.Strategies;
using OrderApi.Infrastructure.Auth;
using OrderApi.Infrastructure.Decorators;
using OrderApi.Infrastructure.Factories;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Repositories;
using OrderApi.Middleware;
using OrderApi.Security;
using OrderApi.Security.Handlers;
using OrderApi.Security.Policies;

var builder = WebApplication.CreateBuilder(args);

// ─── Persistence ──────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("OrderDb"));

// ─── Repositories [Pattern: Repository] [DIP] ─────────────
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<IOrderWriteRepository, OrderRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IOrderReadRepository>(sp =>
    new CachedOrderRepository(
        sp.GetRequiredService<OrderRepository>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<IHttpContextAccessor>()));

builder.Services.AddScoped<IUserRepository, UserRepository>();

// ─── Token Infrastructure [Pattern: Proxy] [SRP] ──────────
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IJwtTokenService>(sp =>
    new JwtTokenValidatorProxy(
        sp.GetRequiredService<JwtTokenService>(),
        sp.GetRequiredService<IMemoryCache>()));

builder.Services.AddSingleton<IRefreshTokenStore, InMemoryTokenStore>();
builder.Services.AddSingleton<IAccessTokenBlacklist, InMemoryTokenStore>();

// ─── OAuth Providers [Pattern: Factory] [OCP] ─────────────
// Register typed HttpClient instances so the providers can receive HttpClient via DI
builder.Services.AddHttpClient<GoogleOAuthProvider>();
builder.Services.AddHttpClient<GitHubOAuthProvider>();
builder.Services.AddScoped<OAuthProviderFactory>();

// ─── Payment Processors [Pattern: Factory] [LSP] ──────────
builder.Services.AddScoped<StripePaymentProcessor>();
builder.Services.AddScoped<PayPalPaymentProcessor>();
builder.Services.AddScoped<PaymentProcessorFactory>();

// ─── Discount Strategies [Pattern: Strategy] [OCP] ────────
builder.Services.AddScoped<NoDiscountStrategy>();
builder.Services.AddScoped<SeasonalDiscountStrategy>();
builder.Services.AddScoped<LoyaltyDiscountStrategy>();

// ─── Application Services [SRP] [DIP] ─────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ─── Authorization [Security: Custom Policies] ────────────
builder.Services.AddScoped<IAuthorizationHandler, AdminOnlyHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrderOwnerHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.AdminOnly, policy =>
        policy.Requirements.Add(new AdminOnlyRequirement()));

    options.AddPolicy(Permissions.OrderOwnerOrAdmin, policy =>
        policy.Requirements.Add(new OrderOwnerRequirement()));
});

// ─── JWT Authentication ────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "OrderApi";
var audience = jwtSettings["Audience"] ?? "OrderApiClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// ─── Swagger with JWT Bearer Support ───────────────────────
builder.Services.AddSwaggerGen(c =>
{
    // Ensure unique schema IDs for types with same short name in different namespaces
    c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Management API",
        Version = "v1",
        Description = """
            Portfolio project demonstrating SOLID principles and design patterns.
            
            Quick start:
            1. POST /api/auth/login with { "email": "admin@orderapi.com", "password": "Admin123!" }
            2. Copy the accessToken from the response
            3. Click the Authorize button and enter: Bearer {paste_token_here}
            4. All order endpoints are now available
            
            Seeded test users:
            - Admin: admin@orderapi.com / Admin123!
            - Customer: customer@orderapi.com / Customer123!
        """
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Tokens expire in 15 minutes. Use /api/auth/refresh to renew."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    c.TagActionsBy(api => new[] { api.HttpMethod });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// ─── CORS: allow Blazor client origins (add other origins if you run the client on different ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7017", "http://localhost:5176")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ─── Seed the database with test data ──────────────────────
SeedDatabase(app.Services);

// ─── Configure the HTTP request pipeline ──────────────────
// [Pattern: Chain of Responsibility] Middleware execution order (outermost to innermost):
app.UseMiddleware<ExceptionHandlingMiddleware>();   // Catches all exceptions
app.UseMiddleware<TokenRevocationMiddleware>();     // Checks blacklisted tokens
app.UseMiddleware<RequestLoggingMiddleware>();      // Logs requests/responses

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
});

// Apply CORS policy so browser-based requests from the Blazor client are allowed.
// This must run before authentication/authorization.
app.UseCors("BlazorClient");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ─── Seed Database Helper ──────────────────────────────────
static void SeedDatabase(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    // Ensure database is created
    dbContext.Database.EnsureCreated();

    // Skip if users already exist
    if (dbContext.Users.Any())
    {
        return;
    }

    // Helper to hash passwords consistently
    static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hashedBytes);
    }

    // Seed Admin User
    var adminId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    var adminUser = new User
    {
        Id = adminId,
        Email = "admin@orderapi.com",
        PasswordHash = HashPassword("Admin123!"),
        Role = UserRole.Admin,
        CreatedAt = DateTime.UtcNow
    };
    dbContext.Users.Add(adminUser);

    // Seed Customer User
    var customerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    var customerUser = new User
    {
        Id = customerId,
        Email = "customer@orderapi.com",
        PasswordHash = HashPassword("Customer123!"),
        Role = UserRole.Customer,
        CreatedAt = DateTime.UtcNow
    };
    dbContext.Users.Add(customerUser);

    // Seed Customer Profile
    var customer = new Customer
    {
        Id = customerId,
        Name = "John Doe",
        Email = "customer@orderapi.com",
        TotalOrdersPlaced = 0
    };
    dbContext.Customers.Add(customer);

    // Seed Sample Orders for Customer
    var order1 = new OrderApi.Domain.Entities.Order
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        CustomerId = customerId,
        Status = OrderStatus.Confirmed,
        TotalAmount = 149.99m,
        CreatedAt = DateTime.UtcNow.AddDays(-2),
        Items = new List<OrderItem>
        {
            new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Sample Product A",
                Quantity = 2,
                UnitPrice = 49.99m
            },
            new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Sample Product B",
                Quantity = 1,
                UnitPrice = 50.01m
            }
        }
    };

    var order2 = new OrderApi.Domain.Entities.Order
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
        CustomerId = customerId,
        Status = OrderStatus.Pending,
        TotalAmount = 99.99m,
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        Items = new List<OrderItem>
        {
            new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Sample Product C",
                Quantity = 1,
                UnitPrice = 99.99m
            }
        }
    };

    dbContext.Orders.Add(order1);
    dbContext.Orders.Add(order2);

    dbContext.SaveChanges();
}