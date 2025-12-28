using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.Application.UseCases.Orders.Queries;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Repositories;

namespace OrderManagement.Tests;

public class OrderCqrsTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
        });
        services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        services.AddMediatR(typeof(CreateOrderCommand).Assembly);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Fact]
    public async Task Create_Order_Should_Return_Success_Result()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 50.00m },
                new() { ProductName = "Product 2", Quantity = 1, UnitPrice = 30.00m }
            }
        };

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CustomerName.Should().Be("John Doe");
        result.Value.CustomerEmail.Should().Be("john@example.com");
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalAmount.Should().Be(130.00m);
    }

    [Fact]
    public async Task Create_Order_Should_Generate_OrderNumber()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderNumber.Should().StartWith("ORD-");
        result.Value.OrderNumber.Should().Contain(DateTime.UtcNow.Year.ToString());
    }

    [Fact]
    public async Task Create_Order_With_Invalid_Data_Should_Return_Failure()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "",
            CustomerEmail = "invalid-email",
            Items = new List<CreateOrderItemDto>()
        };

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Read_Order_By_Id_Should_Return_Order()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Bob Johnson",
            CustomerEmail = "bob@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product A", Quantity = 3, UnitPrice = 15.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var query = new GetOrderByIdQuery { Id = orderId };
        var queryResult = await _mediator.Send(query);

        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Should().NotBeNull();
        queryResult.Value!.Id.Should().Be(orderId);
        queryResult.Value.CustomerName.Should().Be("Bob Johnson");
        queryResult.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Read_NonExistent_Order_Should_Return_Failure()
    {
        var query = new GetOrderByIdQuery { Id = Guid.NewGuid() };

        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NOT_FOUND");
    }

    [Fact]
    public async Task Read_All_Orders_Should_Return_List()
    {
        var command1 = new CreateOrderCommand
        {
            CustomerName = "Alice Brown",
            CustomerEmail = "alice@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product X", Quantity = 1, UnitPrice = 20.00m }
            }
        };

        var command2 = new CreateOrderCommand
        {
            CustomerName = "Charlie Davis",
            CustomerEmail = "charlie@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product Y", Quantity = 2, UnitPrice = 15.00m }
            }
        };

        await _mediator.Send(command1);
        await _mediator.Send(command2);

        var query = new GetOrdersQuery();
        var queryable = await _mediator.Send(query);

        queryable.Should().NotBeNull();
        var orders = queryable.ToList();
        orders.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task Update_Order_Should_Modify_Data()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Old Name",
            CustomerEmail = "old@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var updateCommand = new UpdateOrderCommand
        {
            Id = orderId,
            CustomerName = "New Name",
            CustomerEmail = "new@example.com",
            Status = OrderStatus.Processing
        };

        var updateResult = await _mediator.Send(updateCommand);

        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().NotBeNull();
        updateResult.Value!.CustomerName.Should().Be("New Name");
        updateResult.Value.CustomerEmail.Should().Be("new@example.com");
        updateResult.Value.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task Update_Order_Status_Should_Persist()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var updateCommand = new UpdateOrderCommand
        {
            Id = orderId,
            Status = OrderStatus.Completed
        };

        var updateResult = await _mediator.Send(updateCommand);

        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value!.Status.Should().Be(OrderStatus.Completed);

        var query = new GetOrderByIdQuery { Id = orderId };
        var queryResult = await _mediator.Send(query);

        queryResult.Value!.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Update_NonExistent_Order_Should_Return_Failure()
    {
        var command = new UpdateOrderCommand
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Completed
        };

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NOT_FOUND");
    }

    [Fact]
    public async Task Update_Order_With_Invalid_Data_Should_Return_Failure()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var updateCommand = new UpdateOrderCommand
        {
            Id = orderId,
            CustomerEmail = "invalid-email"
        };

        var result = await _mediator.Send(updateCommand);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Delete_Order_Should_Remove_From_Database()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Delete Me",
            CustomerEmail = "delete@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var deleteCommand = new DeleteOrderCommand { Id = orderId };
        var deleteResult = await _mediator.Send(deleteCommand);

        deleteResult.IsSuccess.Should().BeTrue();
        deleteResult.Value.Should().BeTrue();

        var query = new GetOrderByIdQuery { Id = orderId };
        var queryResult = await _mediator.Send(query);

        queryResult.IsSuccess.Should().BeFalse();
        queryResult.Errors.Should().Contain(e => e.Code == "NOT_FOUND");
    }

    [Fact]
    public async Task Delete_Order_With_Items_Should_Remove_All()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m },
                new() { ProductName = "Product 2", Quantity = 1, UnitPrice = 15.00m },
                new() { ProductName = "Product 3", Quantity = 3, UnitPrice = 5.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        var orderId = createResult.Value!.Id;

        var deleteCommand = new DeleteOrderCommand { Id = orderId };
        var deleteResult = await _mediator.Send(deleteCommand);

        deleteResult.IsSuccess.Should().BeTrue();

        var query = new GetOrderByIdQuery { Id = orderId };
        var queryResult = await _mediator.Send(query);

        queryResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_NonExistent_Order_Should_Return_Failure()
    {
        var command = new DeleteOrderCommand { Id = Guid.NewGuid() };

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NOT_FOUND");
    }

    [Fact]
    public async Task Complete_CRUD_Workflow_Should_Work()
    {
        var createCommand = new CreateOrderCommand
        {
            CustomerName = "Workflow Test",
            CustomerEmail = "workflow@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product A", Quantity = 1, UnitPrice = 100.00m }
            }
        };

        var createResult = await _mediator.Send(createCommand);
        createResult.IsSuccess.Should().BeTrue();
        var orderId = createResult.Value!.Id;

        var readQuery = new GetOrderByIdQuery { Id = orderId };
        var readResult = await _mediator.Send(readQuery);
        readResult.IsSuccess.Should().BeTrue();
        readResult.Value!.CustomerName.Should().Be("Workflow Test");

        var updateCommand = new UpdateOrderCommand
        {
            Id = orderId,
            CustomerName = "Updated Name",
            Status = OrderStatus.Processing
        };
        var updateResult = await _mediator.Send(updateCommand);
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value!.CustomerName.Should().Be("Updated Name");
        updateResult.Value.Status.Should().Be(OrderStatus.Processing);

        var deleteCommand = new DeleteOrderCommand { Id = orderId };
        var deleteResult = await _mediator.Send(deleteCommand);
        deleteResult.IsSuccess.Should().BeTrue();

        var verifyQuery = new GetOrderByIdQuery { Id = orderId };
        var verifyResult = await _mediator.Send(verifyQuery);
        verifyResult.IsSuccess.Should().BeFalse();
    }
}
