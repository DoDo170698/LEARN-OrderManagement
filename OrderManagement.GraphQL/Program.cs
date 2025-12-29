using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.UseCases.Auth.Commands;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Interfaces;
using OrderManagement.GraphQL.GraphQL.DataLoaders;
using OrderManagement.GraphQL.GraphQL.Filters;
using OrderManagement.GraphQL.GraphQL.Mutations;
using OrderManagement.GraphQL.GraphQL.Queries;
using OrderManagement.GraphQL.GraphQL.Types;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAutoMapper(typeof(CreateOrderCommand).Assembly, typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);
builder.Services.AddMediatR(typeof(CreateOrderCommand).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContextPool<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5101",  // Blazor HTTP
                "https://localhost:7067", // Blazor HTTPS
                "http://localhost:5118",  // GraphQL HTTP
                "https://localhost:7178"  // GraphQL HTTPS
              )
              .WithHeaders("Authorization", "Content-Type", "GraphQL-Preflight")
              .WithMethods("POST", "OPTIONS")
              .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<OrderQueries>()
    .AddMutationType()
    .AddDataLoader<OrderTotalAmountDataLoader>()
    .AddDataLoader<OrderItemCountDataLoader>()
    .AddTypeExtension<OrderMutations>()
    .AddSubscriptionType<OrderSubscriptions>()
    .AddType<OrderType>()
    .AddType<OrderItemType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddInMemorySubscriptions()
    .AddAuthorization()
    .AddErrorFilter<ErrorFilter>()
    .ModifyCostOptions(options =>
    {
        options.EnforceCostLimits = true;
        options.MaxFieldCost = 5000;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await context.Database.EnsureCreatedAsync();
    await DataSeeder.SeedDataAsync(context, logger);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

app.MapPost("/api/auth/token", async (IMediator mediator, IConfiguration config) =>
{
    var command = new GenerateTokenCommand(
        config["Jwt:SecretKey"]!,
        config["Jwt:Issuer"]!,
        config["Jwt:Audience"]!
    );
    var result = await mediator.Send(command);
    return Results.Ok(result);
});

app.MapGraphQL();

app.Logger.LogInformation("================================");
app.Logger.LogInformation("GraphQL Server Started");
app.Logger.LogInformation("================================");
app.Logger.LogInformation("GraphQL Endpoint: http://localhost:5118/graphql");
app.Logger.LogInformation("");
app.Logger.LogInformation("Database: SQLite (Databases/ordermanagement.db)");
app.Logger.LogInformation("Architecture: Clean Architecture + CQRS + MediatR");
app.Logger.LogInformation("");
app.Logger.LogInformation("Token Details:");
app.Logger.LogInformation("  - User: Mock Admin User");
app.Logger.LogInformation("  - Email: admin@example.com");
app.Logger.LogInformation("  - Role: Admin");
app.Logger.LogInformation("  - Expires: 2035 (long-lived for testing)");
app.Logger.LogInformation("");
app.Logger.LogInformation("Use Banana Cake Pop or any GraphQL client to test");
app.Logger.LogInformation("================================");

app.Run();
