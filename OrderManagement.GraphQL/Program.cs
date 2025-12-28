using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application;
using OrderManagement.GraphQL.GraphQL.DataLoaders;
using OrderManagement.GraphQL.GraphQL.Filters;
using OrderManagement.GraphQL.GraphQL.Mutations;
using OrderManagement.GraphQL.GraphQL.Queries;
using OrderManagement.GraphQL.GraphQL.Types;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var staticToken = builder.Configuration["Jwt:StaticToken"]!;

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

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
              .AllowAnyHeader()
              .AllowAnyMethod()
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
    await context.Database.EnsureCreatedAsync();
    await DataSeeder.SeedDataAsync(context);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapGraphQL();

Console.WriteLine("================================");
Console.WriteLine("GraphQL Server Started");
Console.WriteLine("================================");
Console.WriteLine($"GraphQL Endpoint: http://localhost:5118/graphql");
Console.WriteLine("");
Console.WriteLine("Database: SQLite (Databases/ordermanagement.db)");
Console.WriteLine("Architecture: Clean Architecture + CQRS + MediatR");
Console.WriteLine("");
Console.WriteLine("Token Details:");
Console.WriteLine("  - User: Mock Admin User");
Console.WriteLine("  - Email: admin@example.com");
Console.WriteLine("  - Role: Admin");
Console.WriteLine("  - Expires: 2035 (long-lived for testing)");
Console.WriteLine("");
Console.WriteLine("Use Banana Cake Pop or any GraphQL client to test");
Console.WriteLine("================================");

app.Run();
