using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.Application.Validators;

namespace OrderManagement.Tests;

public class XssProtectionTests
{
    private readonly CreateOrderCommandValidator _createValidator;
    private readonly UpdateOrderCommandValidator _updateValidator;

    public XssProtectionTests()
    {
        _createValidator = new CreateOrderCommandValidator();
        _updateValidator = new UpdateOrderCommandValidator();
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("Test<script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("John<>Doe")]
    [InlineData("O'Reilly & Sons")]
    [InlineData("Test\"Quote")]
    public void CreateOrder_Should_Reject_Dangerous_Characters_In_CustomerName(string dangerousName)
    {
        var command = new CreateOrderCommand
        {
            CustomerName = dangerousName,
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Input contains dangerous characters (< > & \" ')");
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("Product<script>")]
    [InlineData("<img src=x>")]
    [InlineData("Item<>Name")]
    [InlineData("Test&Product")]
    public void CreateOrder_Should_Reject_Dangerous_Characters_In_ProductName(string dangerousProduct)
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = dangerousProduct, Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].ProductName")
            .WithErrorMessage("Input contains dangerous characters (< > & \" ')");
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Jean-Pierre")]
    [InlineData("José García")]
    [InlineData("李明")]
    [InlineData("Müller")]
    public void CreateOrder_Should_Accept_Safe_CustomerNames(string safeName)
    {
        var command = new CreateOrderCommand
        {
            CustomerName = safeName,
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerName);
    }

    [Theory]
    [InlineData("Laptop")]
    [InlineData("iPhone 15")]
    [InlineData("USB-C Cable")]
    [InlineData("Book: Clean Code")]
    [InlineData("Coffee - 1kg")]
    public void CreateOrder_Should_Accept_Safe_ProductNames(string safeProduct)
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = safeProduct, Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor("Items[0].ProductName");
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("Test<>Name")]
    [InlineData("Name&Company")]
    public void UpdateOrder_Should_Reject_Dangerous_Characters_In_CustomerName(string dangerousName)
    {
        var command = new UpdateOrderCommand
        {
            Id = Guid.NewGuid(),
            CustomerName = dangerousName
        };

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Input contains dangerous characters (< > & \" ')");
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Updated Name")]
    [InlineData("José García")]
    public void UpdateOrder_Should_Accept_Safe_CustomerNames(string safeName)
    {
        var command = new UpdateOrderCommand
        {
            Id = Guid.NewGuid(),
            CustomerName = safeName
        };

        var result = _updateValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void CreateOrder_Should_Reject_Multiple_Items_With_Dangerous_Characters()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Safe Product", Quantity = 1, UnitPrice = 10 },
                new() { ProductName = "<script>alert(1)</script>", Quantity = 1, UnitPrice = 10 },
                new() { ProductName = "Another Safe", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[1].ProductName");
    }

    [Fact]
    public void CreateOrder_Should_Reject_When_Both_CustomerName_And_ProductName_Are_Dangerous()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "<script>evil</script>",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "<img src=x>", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor("Items[0].ProductName");
    }
}
