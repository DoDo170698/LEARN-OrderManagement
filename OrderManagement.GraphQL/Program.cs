using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application;
using OrderManagement.GraphQL.GraphQL.Filters;
using OrderManagement.GraphQL.GraphQL.Mutations;
using OrderManagement.GraphQL.GraphQL.Queries;
using OrderManagement.GraphQL.GraphQL.Types;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Read JWT Configuration from appsettings.json
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var staticToken = builder.Configuration["Jwt:StaticToken"]!;

// Add Infrastructure Layer (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Layer (AutoMapper, FluentValidation, CQRS Handlers)
builder.Services.AddApplication();

// Add AutoMapper for GraphQL layer (Input -> Command mapping)
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add CORS with restricted policy
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

// Add JWT Authentication
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

// Add GraphQL Server with HotChocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<OrderQueries>()
    .AddMutationType()
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
    // FIX: Disable cost enforcement to prevent "maximum allowed field cost exceeded" error
    .ModifyCostOptions(options =>
    {
        options.EnforceCostLimits = false; // Disable cost limit enforcement
        options.MaxFieldCost = int.MaxValue; // Set to maximum value as backup
    });

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Seed mock data for development/testing (centralized in DataSeeder.cs)
    await DataSeeder.SeedDataAsync(context);
}

// Configure middleware pipeline
app.UseCors();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use WebSockets for GraphQL subscriptions
app.UseWebSockets();

// Map GraphQL endpoint
app.MapGraphQL();

Console.WriteLine("================================");
Console.WriteLine("GraphQL Server Started");
Console.WriteLine("================================");
Console.WriteLine($"GraphQL Endpoint: http://localhost:5118/graphql");
Console.WriteLine("");
Console.WriteLine("Database: SQLite (Databases/ordermanagement.db)");
Console.WriteLine("Architecture: Clean Architecture + CQRS + MediatR");
Console.WriteLine("Auth: JWT Bearer Token (Static)");
Console.WriteLine("");
Console.WriteLine("JWT Token (copy this):");
Console.WriteLine(staticToken);
Console.WriteLine("");
Console.WriteLine("Authorization Header:");
Console.WriteLine($"Bearer {staticToken}");
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
