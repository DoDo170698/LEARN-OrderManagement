using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.Application.Validators;

namespace OrderManagement.Tests.ValidatorTests;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_CustomerName_Is_Empty()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "invalid-email",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public void Should_Have_Error_When_Items_Is_Empty()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>()
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10 }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
