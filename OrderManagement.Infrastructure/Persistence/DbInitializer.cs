using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Database initializer for ensuring database is created and seeded
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database, applying migrations and seeding data
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
    }
}
