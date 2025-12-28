using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Seeds mock data for testing and development
/// </summary>
public static class DataSeeder
{
    private static readonly Random _random = new Random();

    private static readonly string[] _firstNames =
    {
        "John", "Jane", "Michael", "Sarah", "David", "Emma", "James", "Olivia",
        "Robert", "Sophia", "William", "Ava", "Richard", "Isabella", "Thomas", "Mia",
        "Charles", "Charlotte", "Daniel", "Amelia", "Matthew", "Harper", "Anthony", "Evelyn",
        "Mark", "Abigail", "Donald", "Emily", "Steven", "Elizabeth"
    };

    private static readonly string[] _lastNames =
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
    };

    private static readonly string[] _products =
    {
        "Laptop Dell XPS 15", "iPhone 15 Pro Max", "Samsung Galaxy S24 Ultra", "iPad Air M2",
        "MacBook Pro 16\"", "Sony WH-1000XM5 Headphones", "Apple Watch Series 9", "AirPods Pro",
        "Logitech MX Master 3S Mouse", "Mechanical Keyboard RGB", "4K Monitor 27\"", "Gaming Chair",
        "USB-C Hub", "External SSD 1TB", "Wireless Charger", "Phone Case Premium",
        "Screen Protector", "Laptop Bag", "Webcam 4K", "Microphone USB",
        "Tablet Stand", "Stylus Pen", "Power Bank 20000mAh", "HDMI Cable 2m",
        "Ethernet Cable Cat8", "Wi-Fi Router", "Smart Speaker", "Security Camera",
        "Smart Light Bulb", "Bluetooth Speaker", "Gaming Mouse", "Monitor Arm Mount",
        "Docking Station", "Portable Monitor 15.6\"", "Graphics Tablet", "Drawing Pen Display"
    };

    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (context.Orders.Any())
        {
            Console.WriteLine("Database already seeded. Skipping...");
            return;
        }

        Console.WriteLine("Seeding database with mock data...");

        var orders = new List<Order>();
        var startDate = DateTime.UtcNow.AddMonths(-6);

        // Create 60 orders with varying statuses and dates
        for (int i = 1; i <= 60; i++)
        {
            var firstName = _firstNames[_random.Next(_firstNames.Length)];
            var lastName = _lastNames[_random.Next(_lastNames.Length)];
            var customerName = $"{firstName} {lastName}";
            var customerEmail = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";

            // Random date within last 6 months
            var createdAt = startDate.AddDays(_random.Next(0, 180)).AddHours(_random.Next(0, 24));

            // Status distribution: 20% Pending, 30% Processing, 40% Completed, 10% Cancelled
            var statusRoll = _random.Next(100);
            OrderStatus status;
            if (statusRoll < 20) status = OrderStatus.Pending;
            else if (statusRoll < 50) status = OrderStatus.Processing;
            else if (statusRoll < 90) status = OrderStatus.Completed;
            else status = OrderStatus.Cancelled;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{Guid.NewGuid():N}",
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddHours(_random.Next(1, 48)),
                Items = new List<OrderItem>()
            };

            // Add 1-8 items per order
            int itemCount = _random.Next(1, 9);
            for (int j = 0; j < itemCount; j++)
            {
                var product = _products[_random.Next(_products.Length)];
                var quantity = _random.Next(1, 6);
                var unitPrice = _random.Next(10, 2000) + (_random.Next(0, 100) / 100m);

                var item = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductName = product,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Subtotal = quantity * unitPrice,
                    CreatedAt = createdAt.AddMinutes(_random.Next(1, 30)),
                    Order = order
                };

                order.Items.Add(item);
            }

            orders.Add(order);
        }

        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();

        Console.WriteLine($"✓ Seeded {orders.Count} orders with {orders.Sum(o => o.Items.Count)} items");
        Console.WriteLine($"  - Pending: {orders.Count(o => o.Status == OrderStatus.Pending)}");
        Console.WriteLine($"  - Processing: {orders.Count(o => o.Status == OrderStatus.Processing)}");
        Console.WriteLine($"  - Completed: {orders.Count(o => o.Status == OrderStatus.Completed)}");
        Console.WriteLine($"  - Cancelled: {orders.Count(o => o.Status == OrderStatus.Cancelled)}");
    }
}
